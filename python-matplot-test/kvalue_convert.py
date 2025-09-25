import os
import pyodbc
from sqlalchemy import create_engine, text
from sqlalchemy.dialects.mysql import insert as mysql_insert
from sqlalchemy.orm import sessionmaker
from dotenv import load_dotenv
import pandas as pd
from datetime import datetime, timedelta ,date
import urllib
import threading
import time
import shelve
from multiprocessing import Lock ,Queue, Process


# 宣告既有資料庫連線參數
MS_Server = "192.168.200.52"
MS_Database = "ASRS_HTBI"
MS_dbuid = "HTBI_MES"
MS_dbpwd = "mes123"

MYSQL_HOST = "192.168.3.100"
MYSQL_USER = "root"
MYSQL_PASSWORD = "Admin0331"
MYSQL_DATABASE = "mes"

log_path = "kvaluefupdate.txt"

#MSQL更新list
kvalueforprodinfo_list = []
mysql_kvalue_list = []

# 定義鎖
log_lock = Lock()

# 載入 .env 檔案
load_dotenv()

# ---------- 設定連線 ----------

# MSSQL 設定（使用 pymssql 驅動）
MSSQL_CONN_STR = (
    f"mssql+pyodbc://{MS_dbuid}:{MS_dbpwd}@{MS_Server}/{MS_Database}"
    "?driver=ODBC+Driver+17+for+SQL+Server"
)
# MySQL 設定（使用 mysql-connector 驅動）
MYSQL_CONN_STR = f"mysql+mysqlconnector://{MYSQL_USER}:{MYSQL_PASSWORD}@{MYSQL_HOST}/{MYSQL_DATABASE}"


# ---------- SQLAlchemy Engine ----------
mssql_engine = create_engine(MSSQL_CONN_STR, pool_size=5, max_overflow=10)
mysql_engine = create_engine(MYSQL_CONN_STR, pool_size=5, max_overflow=10)


def save_last_id(max_id):
    with shelve.open('state.db') as db:
        db['last_max_id'] = max_id

def load_last_id():
    with shelve.open('state.db') as db:
        return db.get('last_max_id', 0)
    

def prev_init_lasttask_iD():                
    # 1. 載入（從檔案、資料庫、shelve 等方式）
     last_max_id = load_last_id()  # 讀出可能是 None、空字串或數字

    # 2. 運用條件式設預設值
    #not last_max_id 會對空字串、0、None 等都視為 True，就是 Python 常見的「falsy」概念
    #  last_max_id = 2046268 if not last_max_id else last_max_id
     last_max_id = last_max_id or 2046268

    #測試修改ID
    # if last_max_id is not None:
    #    print(f"有修正過ID")       
    #    last_max_id = 4543715

     print(f"last_max_id = {last_max_id}")
     return last_max_id

def writer_kvaluelog (log_queue: Queue):
    global log_path 
    with open(log_path, "a", encoding="utf-8") as log_file:
        while True:
            log_msg = log_queue.get()  # 等待從 Queue 中取得 log 訊息
            if log_msg == 'END':  # 收到 'END' 表示退出
                break
            log_file.write(log_msg)
     

def fetch_grouped_data():
    global mysql_kvalue_list, kvalueforprodinfo_list 
    #先擷取上次作業存取的最後一筆 ID (MSSQL資料表-> HTBI_K_Value_MapperType_V)
    last_max_id = prev_init_lasttask_iD()

     # 只保留秒，忽略小數秒
    # target_now = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
    # start_time = f"{target_now} 00:00:00"
    # end_time = f"{target_now} 23:59:59"

    # datetime2(0) 會移除小數秒部分，只保留秒數

    #先找尋當前ID 對應的日期

    query_ackdate = text("""
                SELECT TOP 1 FORMAT(CONVERT(datetime2(0), CREATE_DATE), 'yyyy-MM-dd') as searchID_create_date
                FROM HTBI_K_Value_MapperType_V
                WHERE ID >= :last_max_id
                ORDER BY CREATE_DATE 
                """)
    
    
    query = text("""
                 WITH FilteredData AS (
                    SELECT BOX_BATT, K_value, ID, TRY_CONVERT(date, CREATE_DATE) AS create_date
                    FROM HTBI_K_Value_MapperType_V
                    WHERE TRY_CONVERT(date, CREATE_DATE) IS NOT NULL
                )
                    SELECT BOX_BATT, K_value, ID,create_date
                    FROM FilteredData
                    WHERE create_date >= :target_date
                         AND  ID >= :last_max_id
                    ORDER BY ID 
                """)

    #使用切片`[:]`将原列表的所有元素清空
    kvalueforprodinfo_list[:] = []
    mysql_kvalue_list[:]=[]

    with mssql_engine.connect() as conn:
        #1.先找出日期
        grouped_date = pd.read_sql(query_ackdate, conn, params={                    
            'last_max_id': last_max_id
        })

        if grouped_date.empty:
            raise ValueError(f"No CREATE_DATE found for the given ${last_max_id}")
        
        # 單筆值提取
        fetch_date =  grouped_date.iloc[0]['searchID_create_date']
        print(f"searchID_create_date = {fetch_date}")
        target_date = datetime.strptime(fetch_date, "%Y-%m-%d").date()

        #2.再找出指定欄位(電芯號,K值)

        result = conn.execute(query, {'target_date': target_date, 'last_max_id': last_max_id})
        rows = result.fetchall()
        kvalueforprodinfo_list[:] = [rows]
    
    inner_data = kvalueforprodinfo_list[0]  # 取出裡面那個元素
    print(type(inner_data))  # 看是 tuple 還是 list
    print("查詢筆數為:", len(inner_data))
    # 將 BOX_BATT,K_Value 對應成 MYSQL Column 
    for i,row in enumerate(kvalueforprodinfo_list[0]):
        modleID = str(row[0])
        Kvalue = str(row[1])
        ID = str(row[2])
        create_dt = str(row[3])
        if i == len(kvalueforprodinfo_list[0])-1:            
            print(f"儲存第{len(kvalueforprodinfo_list[0])-1}筆ID：", row)
            save_last_id(ID)  # 儲存最後一筆 ID

        mysql_kvalue_list.append([modleID, Kvalue,ID,create_dt])

def upsert_to_mysql(df: pd.DataFrame , log_queue: Queue):
  global log_path
  log_msgs = []  # 用 list 收集所有 log 訊息  
  with mysql_engine.begin() as conn:
            for index, row in df.iterrows():
                query = text("""
                    INSERT INTO kvalueforprodinfo_update (cell, Kvalue, ID, CREATE_DATE ,updated_at)
                    VALUES (:cell, :Kvalue, :ID, :CREATE_DATE, NOW())
                    ON DUPLICATE KEY UPDATE
                        Kvalue = VALUES(Kvalue),
                        ID = VALUES(ID),
                        CREATE_DATE =  VALUES(CREATE_DATE),
                        updated_at = NOW()
                """)
                conn.execute(query, {                             
                    'cell': row['cell'],
                    'Kvalue': row['K_value'],
                    'ID': row['ID'],
                    'CREATE_DATE': row['create_dt'],
                })

                log_msg = f"[{index+1}/{len(df)}] ✅ Upsert 完成：cell={row['cell']}, Kvalue={row['K_value']}, ID={row['ID']} , CREATE_DATE={row['create_dt']}\n"
                print(log_msg, end='')  # 即時印出到終端機
                log_msgs.append(log_msg)  # 收集訊息
                log_queue.put(log_msg)  # 將 log 訊息放入 queue

  log_queue.put(f"{datetime.now().strftime('%Y-%m-%d %H:%M:%S')} Data sync completed.\n")  # 放入結尾訊息
  log_queue.put('END')  # 用 'END' 告訴 writer 結束

# with open(log_path, "a", encoding="utf-8") as log_file:  # 以追加模式開啟
#         log_file.writelines(log_msgs)
            
# def job():
#     while True:
#         try:
#             main()
#         except Exception as e:
#             print(f"Error during job execution: {e}")
#         time.sleep(3600)  # 睡眠 1 小時	

if __name__ == "__main__":
#   print(pyodbc.drivers())

  log_queue = Queue()
  log_writer = Process(target=writer_kvaluelog, args=(log_queue,))
  log_writer.start()

  fetch_grouped_data()
#   print(f"準備INSERT(UPDATE),mysql_kvalue_list 結果為 = {mysql_kvalue_list}")
  print(f"準備INSERT(UPDATE),mysql_kvalue_list 數量為 = {len(mysql_kvalue_list)}")
  if  len(mysql_kvalue_list) >0:
        df = pd.DataFrame(mysql_kvalue_list, columns=['cell', 'K_value','ID','create_dt'])    
        upsert_to_mysql(df, log_queue)
        print("✅ Data sync completed.")
  else:
        print("⚠️ No data found .")

  log_writer.join()  # 等待 log writer 結束