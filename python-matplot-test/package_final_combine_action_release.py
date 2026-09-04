import os
import pyodbc
import random
from pathlib import Path
from decimal import Decimal, InvalidOperation , ROUND_HALF_UP
from sqlalchemy import create_engine, text ,bindparam
from sqlalchemy.dialects.mysql import insert as mysql_insert
from sqlalchemy.orm import sessionmaker
from dotenv import load_dotenv
import pandas as pd
from datetime import datetime, timedelta ,date
import urllib
import threading
import time
import shelve
import csv
from multiprocessing import Lock ,Queue, Process
from openpyxl import load_workbook


# 宣告既有資料庫連線參數
MS_Server = "192.168.200.52"
MS_Database = "ASRS_HTBI"
MS_dbuid = "HTBI_MES"
MS_dbpwd = "mes123"

MYSQL_HOST = "192.168.3.100"
MYSQL_USER = "root"
MYSQL_PASSWORD = "Admin0331"
MYSQL_DATABASE = "mes"

log_path = "Built_Module_Info.txt"

#MSQL更新list
kvalueforprodinfo_list = []
mysql_kvalue_list = []
extra_caculator_result = []

#MYSQL更新模組號LIST
cellmodle_identitycode =[]

#往前日期數字定義
prefront_number = 1
modle_stand_package_number = 32
process_times = 0

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

#若涵蓋字串型態為datetime 做轉換格式顯示,其餘則保持原先
def DateTime_formatCvt(row):
    return str(tuple(
        value.strftime("%Y-%m-%d %H:%M:%S")
        if isinstance(value, datetime)
        else value
        for value in row
    ))

def DateTime_str_run(value):
    if isinstance(value, datetime):
        return value.strftime("%Y-%m-%d %H:%M:%S")
    return value

def writer_finalpackage_log (log_queue: Queue):

    global log_path

    with open(log_path, "a", encoding="utf-8") as log_file:
        while True:
            try:
                log_msg = log_queue.get()  # 等待從 Queue 中取得 log 訊息

                if log_msg == 'END':  # 收到 'END' 表示退出
                    break
                
                log_file.write(log_msg)
                log_file.flush()

            except KeyboardInterrupt:
                    break

def Fetch_Package_CellRuleData(log_queue: Queue):
    global mysql_kvalue_list, kvalueforprodinfo_list ,cellmodle_identitycode , extra_caculator_result , process_times

    
    #找尋指定的日期(預設前一天),找尋出分配組裝所有電芯資訊參數(32分選號,盒號,K值, '3.45 ,3.5~2.8'電容量 , AC 電壓 內阻值)

    prev_date_str = text("""
                            with orcuj as (
                                     select 
                                           moduleNO as allocate_name,
                                           COUNT(*) AS class_num			   
                                        from mes.stacked_modinfo  
                                        where 
                                             `Time` >= DATE_SUB(CURDATE(), INTERVAL :prefront_number DAY) AND `Time` < DATE_ADD(CURDATE(), INTERVAL :prefront_number DAY)
                                        --   `Time` >= CURDATE() AND  `Time` < DATE_ADD(CURDATE(), INTERVAL :prefront_number DAY)
                                        --   `Time` between '2026-09-01 00:00:00' AND '2026-09-30 23:59:59'
                                        AND NULLIF(TRIM(moduleNO), '') IS NOT NULL
                                        GROUP BY moduleNO
                                 ),
                                class_list AS (
                                        SELECT count( DISTINCT moduleNO ) AS total_class_finalnum,
                                        DATE_FORMAT(
                                        -- 	DATE_SUB(CURDATE(), INTERVAL :prefront_number DAY),
                                        `Time`,'%Y-%m-%d %H:%i:%s'
                                        ) AS date_search_start,
                                        DATE_FORMAT(
                                            DATE_ADD(CURDATE(), INTERVAL :prefront_number DAY),'%Y-%m-%d %H:%i:%s'     
                                        ) AS date_search_end
                                        FROM mes.stacked_modinfo            
                                        WHERE `Time` >= DATE_SUB(CURDATE(), INTERVAL :prefront_number DAY) 
                                        AND `Time` < DATE_ADD(CURDATE(), INTERVAL :prefront_number DAY)
                            --          WHERE `Time` >= CURDATE()
                            --   		AND `Time`  < DATE_ADD(CURDATE(), INTERVAL :prefront_number DAY)
                            --          WHERE `Time` between '2026-09-01 00:00:00' AND '2026-09-30 23:59:59'
                               ),
                               result AS (
                                SELECT
                                    t.class_num,
                                    t.allocate_name,        
                                    p.total_class_finalnum,
                                    p.date_search_start ,
                                    p.date_search_end, 
                                    ROW_NUMBER() OVER (ORDER BY t.class_num DESC) AS rn
                                FROM orcuj t
                                CROSS JOIN class_list p
                               )
                               select 
                                class_num,
                                allocate_name,
                                CASE
                                    WHEN rn = 1 THEN total_class_finalnum
                                    ELSE ""
                                END AS total_class_finalnum,
                                CASE
                                    WHEN rn = 1 THEN date_search_start
                                    ELSE ''
                                END AS date_search_start,
                                CASE
                                    WHEN rn = 1 THEN date_search_end
                                    ELSE ''
                                END AS date_search_end
                            FROM result        
                            order by class_num DESC
                         """)

   
    
    
    #找尋鎖定模組號之關聯電芯組裝資訊(ID, Time, moduleNO, PLCCellID_modinfo, PLCCellIDBeforeV_modinfo, PLCCellIDAfterV_modinfo, Class, REMARK )
    query_rulecell = text ("""
                            select * from mes.stacked_modinfo  
                                WHERE moduleNO = :classcode AND
                                    `Time` >= DATE_SUB(CURDATE(), INTERVAL :prefront_number DAY)  AND `Time` < DATE_ADD(CURDATE(), INTERVAL :prefront_number DAY)
                            --      `Time`  >= CURDATE()   AND  Time < DATE_ADD(CURDATE(), INTERVAL 1 DAY)                            
                            --      `Time` between '2026-09-01 00:00:00' AND '2026-09-30 23:59:59'
                            --       order by DATE_FORMAT(`Time`, '%Y-%m-%d %H:%i:%s')
                                order by id ASC                        
                                limit 32;
                            """)
    
    #使用切片`[:]`将原列表的所有元素清空
    kvalueforprodinfo_list[:] = []
    mysql_kvalue_list[:]=[]
    cellmodle_identitycode[:]=[]
    extra_caculator_result[:]=[]
    
    with mysql_engine.begin() as conn:
        #先取得各項分選結構資訊
        #ex:
           # 各總類量  序號  總     查詢起始               查詢結束
           # '440', 'A47', '45', '2026-08-28 01:06:33', '2026-08-28 00:00:00'
           # '54', 'AC20', '', '', ''
           # '17', 'B25', '', '', ''
           # '16', 'A25', '', '', ''
           # '15', 'O', '', '', ''
           
        allinfo_class = pd.read_sql(prev_date_str, conn, params={                    
            'prefront_number': prefront_number 
        })

        if allinfo_class.empty:
            raise ValueError(f"No ruletable info found for the given ${prefront_number}")
            
        
        # 先行確認總分選類別數量
        total_casenum =  allinfo_class.iloc[0]['total_class_finalnum']
        print(f"總模組號類別數量 = {total_casenum}")
       
        if allinfo_class.empty or int(total_casenum) == 0:
           raise ValueError(f"總模組號類別數量 found not 或 數量為 0")
        
        for _, row in allinfo_class.iterrows():

            #確定模組碼數量後(不為0或空) , 再接續找出指定欄位 (class_num,allocate_name)
            classcode_num = int(row['class_num'])
            allocate_codestr = row['allocate_name']
            
            #符合單一模組號 為32顆電芯 做組裝(測試先用大等於32做模擬 ,忽略 'O')
            if classcode_num >= modle_stand_package_number and allocate_codestr.upper() != 'O':
                #將分碼號存入後續查詢
                cellmodle_identitycode.append(str(allocate_codestr).strip())
                
            
        #符合32顆原則執行(電芯號資訊欄位合併)
        for classcode in cellmodle_identitycode:
            time.sleep(0.5) # 等待0.5秒 再執行每一個任務query
            rule_result = conn.execute(query_rulecell, {'classcode': classcode , 'prefront_number': prefront_number})                                                   
            rows = rule_result.fetchall()

            #清空以下清單list
            extra_caculator_result[:]=[]
            packagedata = []
            error_match = []

            #SQLAlchemy expanding=True
            add_directfield_query = text("""
                                           SELECT
                                                s.id,
                                                s.`Time`,
                                                s.PLCCellID_modinfo,
                                                t.K_Value,
                                                t.VAHSB,
                                                t.VAHSC,	
                                                s.Class,
                                                p.PLCTrayID_CE,
                                                k.acirVP12_CE,
                                                k.acirRP12_CE,
                                                s.moduleNO ,
                                                NULL AS last_define_location,
                                                NULL AS parallel_match                               
                                            FROM mes.stacked_modinfo s 

                                            LEFT JOIN mes.testmerge_cc1orcc2 t 
                                                ON t.modelId = s.PLCCellID_modinfo 
                                                AND t.parameter = '017'
                                                
                                            LEFT JOIN mes.stacked_ce p 
                                                ON p.PLCCellID_CE = s.PLCCellID_modinfo  
                                                
                                            LEFT JOIN mes.schk_cellrule_dupfinish_2 k 
                                                ON k.PLCCellID_CE = p.PLCCellID_CE

                                            WHERE
                                                s.moduleNO = :classcode
                                                AND s.`Time` >= DATE_SUB(CURDATE(), INTERVAL :prefront_number DAY)
                                                AND s.`Time` < DATE_ADD(CURDATE(), INTERVAL :prefront_number DAY)                                                
                                          --    AND s.`Time` >= CURDATE() 
                                          --    AND s.`Time` < DATE_ADD(CURDATE(), INTERVAL :prefront_number DAY)
                                          --    AND s.`Time` between '2026-09-01 00:00:00' AND '2026-09-03 23:59:59'
                                                AND s.PLCCellID_modinfo IN :model_id_array
                                            ORDER BY s.id ASC; 
                                        """).bindparams(
                                                bindparam(
                                                    "model_id_array",
                                                    expanding=True
                                                )
                                         )

            #取電芯ID清單 ,並join testmerge_cc1orcc2 指定欄位(K_Value , VAHSB , VAHSC)            
            model_id_list = [dw.PLCCellID_modinfo for dw in rows]
            #model_id_array = '","'.join(model_id_list)
            merge_caseprocess = conn.execute(add_directfield_query, { 'classcode': classcode ,'prefront_number': prefront_number  ,'model_id_array': model_id_list})

            #先全部轉成 list，再處理
            for row in merge_caseprocess:
                item = dict(row._mapping)
                packagedata.append(item)
            
            for i, item in enumerate(packagedata):                                                             

                # 每兩筆增加 1
                item["last_define_location"] = (i // 2) + 1

                #計算並聯阻抗值,並存在前一組欄位
                if i % 2 == 0:

                    continue
                else:
                    #當下ACIR 電阻值                                              
                    z1_Resistance = packagedata[i-1]["acirRP12_CE"]    
                    z2_Resistance = packagedata[i]["acirRP12_CE"]

                    # 阻抗資料檢查有一方無效或空
                    if z1_Resistance is None or z2_Resistance is None:

                        error_match_msg1 = (  
                                f"⚠️ 阻抗資料異常："
                                f"{packagedata[i - 1]['PLCCellID_modinfo']} / "
                                f"{packagedata[i]['PLCCellID_modinfo']}"
                            )
                        
                        error_match.append(error_match_msg1)
                        continue

                    z1_reg_val = Decimal(str(z1_Resistance))
                    z2_reg_val = Decimal(str(z2_Resistance))


                    # 總阻抗計算加總為0
                    if z1_reg_val + z2_reg_val == 0:

                        error_match_msg2 = (  
                            f"⚠️ 阻抗總和為 0："
                            f"{packagedata[i - 1]['PLCCellID_modinfo']} / "
                            f"{packagedata[i]['PLCCellID_modinfo']}"
                        )

                        error_match.append(error_match_msg2)
                        continue

                    # --------------------------------------
                    # 並聯阻抗計算
                    # 精確到小數第 5 位，四捨五入
                    # --------------------------------------

                    zMatch_Resist = (
                        z1_reg_val * z2_reg_val / (z1_reg_val + z2_reg_val)
                    ).quantize(
                        Decimal("0.00001"),
                        rounding=ROUND_HALF_UP
                    )
                    
                    #計算完總阻抗要傳存到上一個data i 位置
                    packagedata[i-1]["parallel_match"] =  zMatch_Resist
                    packagedata[i]["parallel_match"] =  ""

            # log_msg = (         
            #     f"---------------start---------------\n"       
            #     f"查詢分選碼為: {classcode}\n"
            #     f"筆數為: {len(rows)}\n"
            #     f"資料流為:\n"
            #     + "\n".join(DateTime_formatCvt(dw) for dw in rows)
            #     + "\n"     
            #     f"---------------end---------------\n"       
            # )
            
            # Console
            #print(log_msg, end='')
            #先行確認
            # print(
            #     "\n".join(
            #         DateTime_str_run(tmda["Time"])
            #         for tmda in packagedata
            #     )
            # )

            for tmda in packagedata:
                tmda["Time"] = DateTime_str_run(tmda["Time"])

            # print(
            #     "\n".join(
            #         str(dw)
            #         for dw in packagedata
            #     )
            # )

            log_msg = (
                "\n".join(
                    str(dw)
                    for dw in packagedata
                )
            )
            
            # Log
            log_queue.put(log_msg)

            #upsert 寫存入目標資料庫表單後續追蹤
            df_one = pd.DataFrame(packagedata)
            upsert_to_packagefinal(df_one, log_queue)

            #執行classID 編輯更新資料庫次數
            process_times = process_times + 1

# ==========================================
# ACIR 工程單位轉換
# scientific notation → Decimal 5位小數
# ==========================================
def convert_acir_decimal(value):

    if pd.isna(value) or value == "":
        return None

    try:
        #預設浮點5位數精度
        return Decimal(str(value)).quantize(
            Decimal("0.00001"),
            rounding=ROUND_HALF_UP
        )

    except (InvalidOperation, ValueError, TypeError):
        return None

#產生xlsx
def export_xlsx_processrun (log_queue: Queue):

    final_xlsx_result = text ("""
                            select machine_workTime, PLCCellID_modinfo, K_Value, VAHSB, VAHSC, PLCCellIDClass_CE, PLCTrayID_CE, acirVP12_CE, acirRP12_CE, model_combine_number, last_define_location, parallel_match 
                            from mes.total_finalassembly  WHERE
                            --  `machine_workTime`  >= CURDATE()   AND  `machine_workTime` <= DATE_ADD(CURDATE(), INTERVAL :prefront_number DAY)
                                `machine_workTime` >= DATE_SUB(CURDATE(), INTERVAL :prefront_number DAY) AND `machine_workTime` < DATE_ADD(CURDATE(), INTERVAL :prefront_number DAY)
                            --  `machine_workTime` BETWEEN '2026-08-31 00:00:00' AND '2026-08-31 23:59:59'       
                            --  order by DATE_FORMAT(`machine_workTime`, '%Y-%m-%d %H:%i:%s')
                                order by id ASC;
                            """)


    # ==========================================
    # 預設執行-> 前一天日期
    # ==========================================
    previous_date = datetime.now() - timedelta(days=1)
    #date_str = datetime.now().strftime("%Y-%m-%d")
    date_str = previous_date.strftime("%Y-%m-%d") 
    
    with mysql_engine.begin() as conn:
         
        allprev_1day_package = pd.read_sql(final_xlsx_result, conn, params={                    
            'prefront_number': prefront_number 
        })
 
        if allprev_1day_package.empty:
            raise ValueError(f"No test_finalpackage info found for the given date: ${previous_date}")

        #確定資料結構為DataFrame
        # work_time = pd.to_datetime(
        #         allprev_1day_package["machine_workTime"],
        #         errors="coerce"
        # )

        # get_strtime = work_time.dropna()

        # #讀取machine_workTime欄位 (machine_workTime) 轉換日期,若不為正常表示式則使用前日日期
        # if not get_strtime.empty and isinstance(get_strtime.iloc[0], datetime):
        #     date_str = get_strtime.iloc[0].strftime("%Y-%m-%d")

        #使用data列走訪判斷
        for _, dtime in allprev_1day_package.iterrows():
            work_time = dtime["machine_workTime"]
            # print("work_time type = " , type(work_time))
            # print("work_time 字串為 = " , str(work_time))
            if isinstance(work_time, datetime):
                date_str = work_time.strftime("%Y-%m-%d")
                break
            else:
                try:
                    date_str = datetime.strptime(
                            work_time.strip(),
                            "%Y-%m-%d %H:%M:%S"
                        ).strftime("%Y-%m-%d")

                    break
                except ValueError:
                   continue
  

    output_dir = (Path(r"C:\BatteryAssembly_Final")/ previous_date.strftime("%Y"))
        
    if not os.path.isdir(output_dir):       
       os.makedirs(output_dir, exist_ok=True)

    # ==========================================
    # XLSX 檔名
    # ==========================================
    output_file = (
        output_dir
        / f"PLCCellID_modinfo_spec_{date_str}_非連續_finalpackage.xlsx"
    )

    #將工程表示單位轉為浮點數顯示(驗證比對需要)
    allprev_1day_package["acirVP12_CE"] = (
        allprev_1day_package["acirVP12_CE"]
        .apply(convert_acir_decimal)
    )

    allprev_1day_package["acirRP12_CE"] = (
        allprev_1day_package["acirRP12_CE"]
        .apply(convert_acir_decimal)
    )

    #將既有欄位辨識切換,後續若要取原先SQL鍵名取值不受影響
    xlsx_df = allprev_1day_package.rename(
      columns={
        'VAHSB': '3.45V_mAH',
        'VAHSC': '3.5-2.8V_mAH'
       }
    )  

    # ==========================================
    # DataFrame → XLSX
    # ==========================================    
    xlsx_df.to_excel(
        output_file,
        index=False,
        engine="openpyxl"
    )

    log_xlsx_msg = (
        f"✅ 匯出 XLSX 完成，"
        f"總資料筆數：{len(allprev_1day_package)}，"
        f"檔案：{output_file}\n"
    )

    #針對excel 欄位 精度需要再detial 強制設置位數
    wb = load_workbook(output_file)
    ws = wb.active

    # 找欄位名稱
    header_map = {
        ws.cell(1, col).value: col
        for col in range(1, ws.max_column + 1)
    }

    for field in ["acirVP12_CE", "acirRP12_CE"]:
        col = header_map.get(field)

        if col:
            for row in range(2, ws.max_row + 1):
                ws.cell(row=row, column=col).number_format = "0.00000"

    wb.save(output_file)

    print(log_xlsx_msg, end="")

    log_queue.put(log_xlsx_msg)

    log_queue.put(f" 匯出xlsx完成 , 總資料筆數：" f"{len(allprev_1day_package)}")   
 
                
def upsert_to_packagefinal(df: pd.DataFrame , log_queue: Queue):
  
  global log_path

  log_msgs = []  # 用 list 收集所有 log 訊息  

  query = text("""
                      INSERT INTO mes.total_finalassembly ( allocate_datetime , rule_id , machine_workTime , PLCCellID_modinfo , 
                                                            K_Value , VAHSB , VAHSC , 
                                                            PLCCellIDClass_CE , PLCTrayID_CE , acirVP12_CE , acirRP12_CE , 
                                                            model_combine_number , last_define_location , parallel_match 								  
                                                        )									  
                      VALUES ( NOW() , :rule_id , :machine_workTime , :PLCCellID_modinfo , 
                            :K_Value  , :VAHSB , :VAHSC  , 
                            :PLCCellIDClass_CE  , :PLCTrayID_CE  , :acirVP12_CE  , :acirRP12_CE  , 
                            :model_combine_number  , :last_define_location  , :parallel_match 
                            )
                      ON DUPLICATE KEY UPDATE
                          allocate_datetime = NOW(),
                          rule_id = VALUES(rule_id),
                          machine_workTime = VALUES(machine_workTime),
                          K_Value = VALUES(K_Value),
                          VAHSB = VALUES(VAHSB),
                          VAHSC = VALUES(VAHSC),
                          PLCCellIDClass_CE = VALUES(PLCCellIDClass_CE),
                          PLCTrayID_CE = VALUES(PLCTrayID_CE),
                          acirVP12_CE = VALUES(acirVP12_CE),
                          acirRP12_CE = VALUES(acirRP12_CE),
                          model_combine_number = VALUES(model_combine_number),
                          last_define_location = VALUES(last_define_location),
                          parallel_match = VALUES(parallel_match)
                  """)
  
  with mysql_engine.begin() as conn:
            for index, row in df.iterrows():               
                conn.execute(query, 
                   {                             
                    'rule_id' : row['id'],
                    'machine_workTime' : row['Time'], 
                    'PLCCellID_modinfo' : row['PLCCellID_modinfo'],
                    'K_Value' : row['K_Value'],
                    'VAHSB' : row['VAHSB'], 
                    'VAHSC' : row['VAHSC'], 
                    'PLCCellIDClass_CE' : row['Class'], 
                    'PLCTrayID_CE' : row['PLCTrayID_CE'],
                    'acirVP12_CE' : row['acirVP12_CE'], 
                    'acirRP12_CE' : row['acirRP12_CE'],
                    'model_combine_number' : row['moduleNO'],
                    'last_define_location' : row['last_define_location'],
                    'parallel_match' : row['parallel_match'],
                   }
                )

                # log_msg = f"[{index+1}/{len(df)}] ✅ Upsert 完成：cell={row['cell']}, Kvalue={row['K_value']}, ID={row['ID']} , CREATE_DATE={row['create_dt']}\n"

                log_msg = (      
                            f"[{index + 1}/{len(df)}] ✅ Upsert 完成：\n"                         
                            f"rule_id = {row['id']}\n"
                            f"machine_workTime = {row['Time']}\n"
                            f"PLCCellID_modinfo = {row['PLCCellID_modinfo']}\n"
                            f"K_Value = {row['K_Value']}\n"
                            f"VAHSB = {row['VAHSB']}\n"
                            f"VAHSC = {row['VAHSC']}\n"
                            f"PLCCellIDClass_CE = {row['Class']}\n"
                            f"PLCTrayID_CE = {row['PLCTrayID_CE']}\n"
                            f"acirVP12_CE = {row['acirVP12_CE']}\n"
                            f"acirRP12_CE = {row['acirRP12_CE']}\n"
                            f"model_combine_number = {row['moduleNO']}\n"
                            f"last_define_location = {row['last_define_location']}\n"
                            f"parallel_match = {row['parallel_match']}\n"
                )
                        
                print(log_msg, end='')  # 即時印出到終端機
                log_msgs.append(log_msg)  # 收集訊息
                log_queue.put(log_msg)  # 將 log 訊息放入 queue

  log_queue.put(f"{datetime.now().strftime('%Y-%m-%d %H:%M:%S')} Data sync completed.\n")  # 放入結尾訊息
    

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
  log_writer = Process(target=writer_finalpackage_log, args=(log_queue,))
  log_writer.start()

  try:

    #執行組裝資料串結各項電芯資訊欄位(最終需要新增結算出 並聯(位置及阻抗值)
    Fetch_Package_CellRuleData(log_queue)

    print(f"程序次數 {int(process_times)}")
    print(f"cellmodle_identitycode classid類別數 : {len(cellmodle_identitycode)}")

    #if  len(cellmodle_identitycode) > 0:
    if int(process_times) == int(len(cellmodle_identitycode)):
        print("➡️ 條件符合，開始產生 XLSX")
        #產生前日結果xlsx
        export_xlsx_processrun(log_queue)
            
    else:
            print("⚠️ No schk_cellrule data found .")

  except KeyboardInterrupt:
        print("\n⚠️ 使用者中止程式 (Ctrl+C)")

  except Exception as e:
        print(f"\n❌ 程式發生錯誤：{e}")
        raise
  
  finally:

    # ==========================
    # 一定會執行
    # ==========================
    # 只由 main 負責通知 writer 結束
    log_queue.put("END")

    # 等待 writer process 完全結束
    log_writer.join()

    print(f"✅ 電芯模組組裝完成 -> total_finalassembly update:{datetime.now().strftime('%Y-%m-%d %H:%M:%S')} cpmplete!")

  
  