#!/usr/bin/python
import os, sys
import pandas as pd
import csv
import numpy as np
from tkinter import Tk
from tkinter.filedialog import askdirectory
import chardet
import glob
import shutil

# chunk = []
# row_count = 0
# file_number = 1
# header = ''
# csv_path_result = os.getcwd()+'/SplitLargeCSV_Result'
# all_csvfiles = []


def main():
    chunk = []
    row_count = 0
    file_number = 1
    header = ''
    csv_path_result = os.getcwd()+'/SplitLargeCSV_Result'
    all_csvfiles = []
    # 初始化 Tkinter
    root = Tk()
    root.withdraw()  # 隱藏主視窗
    #分割後csv 存放路徑
    if not os.path.isdir(csv_path_result):
        os.mkdir(csv_path_result)
    
   # splitpath = int(input("請選擇csv工作資料夾 \n"))
    # 讓用戶選擇資料夾
    folder_path = askdirectory(title="請選擇csv工作資料夾")

    # 如果使用者未選擇資料夾，則退出
    if not folder_path:
        print("未選擇csv工作資料夾，程序退出。")
        exit()

    splitmodel = int(input("請輸入裁切模式 1:(使用pandas) , 2:(使用chunk讀寫row演算)) \n"))

    #使用pandas 套件擷取檔案分析拆分
    if splitmodel == 1:
     # 搜尋指定資料夾下的所有 .csv 檔案
     csv_Allfiles = [f for f in os.listdir(folder_path) if f.endswith('.csv') or f.endswith('.xlsx')]

    # 如果資料夾下沒有 .csv 檔案，則退出
     if not csv_Allfiles:
        print("目前選擇資料夾下沒有 .csv 或 .xlsx 檔案，程序退出。")
        exit()

     # 依次處理每個 CSV 檔案
     for csv_or_xlsx_file in csv_Allfiles:
            file_path = os.path.join(folder_path, csv_or_xlsx_file)
            #擷取附檔名(csv 或 xlsx)
            ext =  os.path.splitext(csv_or_xlsx_file)[1].lower()

            # 將檔案名稱加入列表
            all_csvfiles.append(csv_or_xlsx_file)

            # 檢測檔案編碼
            with open(file_path, 'rb') as file:  # 用 'rb' 模式讀取檔案以便檢測編碼
                raw_data = file.read()
                result = chardet.detect(raw_data)
                file_encoding = result['encoding']
            
            # 讀取 CSV 檔案
            if ext == '.csv':
                df = pd.read_csv(file_path,encoding=file_encoding, on_bad_lines='skip', engine='python')
            elif ext == '.xlsx':
                df = pd.read_excel(file_path)
            else:
                continue  # 忽略未知格式
           

            # 計算每個部分的大小，將資料分為 3 部分
            part_size = len(df) // 3
            
            # 切割資料
            df_part1 = df[:part_size]               # 前 1/3 部分
            df_part2 = df[part_size: 2 * part_size] # 中間 1/3 部分
            df_part3 = df[2 * part_size:]           # 後 1/3 部分
            
            # 新檔案名稱
            base_filename = os.path.splitext(csv_or_xlsx_file)[0]  # 取得原檔名，不含副檔名
            
            # 儲存切割後的檔案

            if ext == '.csv':
                df_part1.to_csv(os.path.join(csv_path_result, f'{base_filename}_part1.csv'), index=False)
                df_part2.to_csv(os.path.join(csv_path_result, f'{base_filename}_part2.csv'), index=False)
                df_part3.to_csv(os.path.join(csv_path_result, f'{base_filename}_part3.csv'), index=False)
            elif ext == '.xlsx':
                df_part1.to_excel(os.path.join(csv_path_result, f'{base_filename}_part1.xlsx'), index=False)
                df_part2.to_excel(os.path.join(csv_path_result, f'{base_filename}_part2.xlsx'), index=False)
                df_part3.to_excel(os.path.join(csv_path_result, f'{base_filename}_part3.xlsx'), index=False)

            print(f"檔案 {csv_or_xlsx_file} 已被拆分並儲存為三個檔案。")

     print(f"所有CSV -> {all_csvfiles}檔案全部處理完成至 -> {csv_path_result} 資料夾。")
                

    #使用讀寫csv 套件擷取檔案依照row數量拆分
    elif splitmodel == 2:
     # 搜尋指定資料夾下的所有 .csv 檔案
     csv_Allfiles = [f for f in os.listdir(folder_path) if f.endswith('.csv') or f.endswith('.xlsx')]

     # 如果資料夾下沒有 .csv 檔案，則退出
     if not csv_Allfiles:
        print("目前選擇資料夾下沒有 .csv 或 .xlsx 檔案，程序退出。")
        exit()

     splitcsv_size = int(input("請輸入分割檔案DataSize尺寸: ex:890000 89萬筆為單位拆分3份(MW電網案例) \n"))

     # 依次處理每個 CSV 檔案
     for csv_or_xlsx_file in csv_Allfiles:
         file_number = 1         
         origin_file_path = os.path.join(folder_path, csv_or_xlsx_file)
         base_filename = os.path.splitext(csv_or_xlsx_file)[0]  # 取得原檔名，不含副檔名         
         ext = os.path.splitext(csv_or_xlsx_file)[1].lower() #檔案屬性(.csv 或 .xlsx)

         # 將檔案名稱加入列表
         all_csvfiles.append(csv_or_xlsx_file)
        
         #初始化存取空間
         header = ''
         chunk = []
         row_count = 0
        
         if ext == '.csv':
            # 檢測檔案編碼
            with open(origin_file_path, 'rb') as file:  # 用 'rb' 模式讀取檔案以便檢測編碼
                raw_data = file.read()
                result = chardet.detect(raw_data)
                file_encoding = result['encoding']

            # 以檢測到的編碼讀取檔案
            with open(origin_file_path,'r', encoding=file_encoding) as file:
                reader = csv.reader(file)            
                for row in reader:
                    if header == '':
                       header = row
                       continue 

                    chunk.append(row)
                    row_count += 1
                    if row_count >= splitcsv_size:                                        
                        cuv_csv_filterpart = os.path.join(csv_path_result, base_filename+'_part'+str(file_number)+'.csv')
                      
                        # newline=''，來避免寫入多餘的空白行
                        with open(cuv_csv_filterpart, 'w', encoding=file_encoding , newline='') as new_file:
                            writer = csv.writer(new_file)
                            writer.writerow(header)
                            for row in chunk:
                                if any(row):
                                  writer.writerow(row)    
                        chunk = []
                        strnumber = str(file_number)
                                           
                        print(f"已經拆分新產生->{base_filename}_part{strnumber}.csv'")

                        file_number +=1
                        row_count = 0
                        

                # 將剩餘的資料寫入新檔案                                                                 
                if len(chunk) > 0:
                    base_filename = os.path.splitext(csv_or_xlsx_file)[0]  # 取得原檔名，不含副檔名
                    cuv_csv_filterpart = os.path.join(csv_path_result, base_filename+'_part'+str(file_number)+'.csv')
                
                    with open(cuv_csv_filterpart, 'w', encoding=file_encoding , newline='') as new_file:
                        writer = csv.writer(new_file)
                        writer.writerow(header)
                        for row in chunk:
                            if any(row):
                               writer.writerow(row)
                            
                strnumber = str(file_number)
                print(f"已經拆分新產生->{base_filename}_part{strnumber}.csv'")
            
         elif ext == '.xlsx':
              df = pd.read_excel(origin_file_path)
              total_rows = len(df)

              for start in range(0, total_rows, splitcsv_size):
                end = start + splitcsv_size
                df_chunk = df.iloc[start:end]
                output_file = os.path.join(csv_path_result, f"{base_filename}_part{file_number}.xlsx")
                df_chunk.to_excel(output_file, index=False)
                print(f"已經拆分新產生 -> {os.path.basename(output_file)}")
                file_number += 1
                   
         print(f"檔案 {csv_or_xlsx_file} 已被拆分{file_number}個檔案。")
           
     print(f"所有CSV -> {all_csvfiles}檔案全部處理完成至 -> {csv_path_result} 資料夾。")
                  

if __name__ == '__main__':
	 main()