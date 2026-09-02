import os
import time
import glob
import shutil
from tkinter import TRUE
from datetime import datetime

# 監控的資料夾
DETEC_VENDER_FOLDERS = [
    r"Y:\\TrayCellData\\R_bak_HTBI\\CC",
    r"Y:\\TrayCellData\\R_bak_HTBI\\PF",
    r"Y:\\TrayCellData\\R_bak_HTBI\\Chroma\\CC",
    r"Y:\\TrayCellData\\R_bak_HTBI\\Chroma\\PF",
]

#本機端環境DEBUG測試
# DETEC_VENDER_FOLDERS = [
#     r"C:\\copy_temp\\SECI_Source_PF",
#     r"C:\\copy_temp\\SECI_Source_CC",
#     r"C:\\copy_temp\\Chroma_Source_PF",
#     r"C:\\copy_temp\\Chroma_Source_CC",
# ]


#半自動或手動目前預設工作路徑
RUNTIME_ACTION_FOLDERS = [
    #r"Y:\\source_pfcc",
    "C:\\copy_temp\\\source_pfcc",    
]


#每個監控資料夾當天執行的檔案名稱做紀錄
SECI_CHROMA_All_thisyear_csv= []


def create_nowyear_folder_path_and_backup():
    """根據當前日期生成資料夾名稱"""
    current_year = datetime.now().strftime("%Y")    
    total_prepare_count = 0 #紀錄總共幾筆要複製的檔案數量

    # 紀錄各來源資料夾處理數量
    folder_statistics = {}
    
    global SECI_CHROMA_All_thisyear_csv

            
     # 檢查每個資料夾中的 CSV 檔案
    for folder_path in DETEC_VENDER_FOLDERS:        
        if check_csv_file_this_year_exists(folder_path) : # 檢查資料夾中是否有符合當年的 CSV 檔案
           year_folder_path = os.path.join(folder_path, current_year) #待備份資料夾(該年度)
           
           # 如果當年資料夾不存在，則創建它    
           if not os.path.exists(year_folder_path):
              print(f"創建資料夾: {year_folder_path}")
              os.makedirs(year_folder_path, exist_ok=True)
        
           #預設每個路徑資料量為0
           folder_statistics[folder_path] = 0

       
           # 以下針對存取的檔案依照該路徑年分資料夾和工作路徑資料夾予以複製並刪除原使路徑放置之檔案
           if len(SECI_CHROMA_All_thisyear_csv) > 0 :
             for csv_origin_analysis in SECI_CHROMA_All_thisyear_csv:
                try:

                  target_file = os.path.join(
                        year_folder_path,
                        os.path.basename(csv_origin_analysis)
                    )

                  #  先確認是否有同樣資料再目標資料夾存在,若不存在再複製檔案到備份當年資料夾
                  if os.path.abspath(csv_origin_analysis) != os.path.abspath(target_file):
                     shutil.copy2(csv_origin_analysis, year_folder_path)
                  
                  # 複製檔案到實際運行路徑資料夾
                  for runtime_folder in RUNTIME_ACTION_FOLDERS:
                    shutil.copy2(csv_origin_analysis, runtime_folder)

                 # 複製完成後刪除原始檔案
                  os.remove(csv_origin_analysis)

                  total_prepare_count +=1
                  folder_statistics[folder_path] +=1
                  print(f"檔案 {csv_origin_analysis} 複製到資料夾 {year_folder_path}", flush=True)


                except Exception as e:
                  print(f"處理檔案 {folder_path} 時出現錯誤: {e}")

    return total_prepare_count , folder_statistics                

# 用來檢查當年的 CSV 檔案
def check_csv_file_this_year_exists(folder_name):
    this_year =  '_'+datetime.now().strftime("%Y")  # 當年，例如 "XX_2026XXX"
  
    SECI_CHROMA_All_thisyear_csv.clear() 
    
    # 檢查指定資料夾下是否有符合條件的 CSV 檔案 (walk 為根目錄全部走訪, listdir 為只搜尋目前目錄 )

    # walk 方法    
    for root, dirs, files in os.walk(folder_name):
        #加下列只搜尋指定路徑
        if root != folder_name:
           continue

        for file in files:        
          if file.endswith(".csv") and this_year in file:
            print(f"檔案 {file} 在資料夾 {folder_name} 中找到，符合當年日期。")
            csv_thisyear_file = os.path.join(root, file)
            SECI_CHROMA_All_thisyear_csv.append(csv_thisyear_file)  # 儲存檔案名稱(含原始路徑)

    # listdir 方法
    # for file in os.listdir(folder_name):
    #     full_path = os.path.join(folder_name, file)
    #     #只搜尋檔案
    #     if os.path.isfile(full_path):
    #         if file.endswith(".csv") and this_year in file:
    #             print(f"檔案 {file} 在資料夾 {folder_name} 中找到，符合當年日期。")              
    #             SECI_CHROMA_All_thisyear_csv.append(full_path)
                
    if len(SECI_CHROMA_All_thisyear_csv) > 0:       
       return True  # 找到符合條件的檔案，返回 True    
    else: 
       return False


if __name__ == "__main__":
    all_count, backup_folder_pfcc_csv_file = (
        create_nowyear_folder_path_and_backup()
    )

    print("\n===== 各來源資料夾處理統計 =====")

    for folder, count in backup_folder_pfcc_csv_file.items():
        print(f"{folder} : {count} 筆")

    print("--------------------------------")

    print(f"一共 {all_count} 筆電化學原始檔案 複製到工作(SECI,CHROMA)資料夾 {datetime.now().strftime('%Y')} 年度")
