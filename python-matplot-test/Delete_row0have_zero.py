import pandas as pd
import os
from tkinter import Tk
from tkinter.filedialog import askdirectory

# 關閉 tkinter 主視窗
Tk().withdraw()

# 選取資料夾
folder_path = askdirectory(title="請選擇 CSV 工作資料夾")


# 如果使用者未選擇資料夾，則退出
if not folder_path:
    print("未選擇資料夾，程序退出。")
    exit()

# 讓使用者選擇資料夾
if folder_path:
    for filename in os.listdir(folder_path):
        if filename.endswith(".csv"):
            file_path = os.path.join(folder_path, filename)
            # 設定 na_values 讓讀入時就視空白為 NaN
           # df = pd.read_csv(file_path, na_values=["", " ", "　"])  # 包含全形空白
            df = pd.read_csv(file_path, na_values=["", " ", "　"], keep_default_na=False)  # 包含全形空白
                    
            # for index, row in df.iterrows():
            #     print(f"Row {index}: {[repr(x) for x in row]}")            

            # 條件 1：第一欄是 '-' 或其他特定值
            col0 = df.iloc[:, 0].astype(str).str.strip()
            cond_col0_is_invalid = col0.isin(["-", "'-'", "'-CC'"])

            # 條件 2：其餘欄位皆為 NaN
            cond_others_all_nan = df.iloc[:, 1:].isna().all(axis=1)

            # 條件 3 刪除第一欄為 0 且其他為空值的列
            cond_equalzerp_others_null= (df.iloc[:, 0] == 0) & df.iloc[:, 1:].isnull().all(axis=1)

            # 合併刪除條件
            rows_to_drop = cond_col0_is_invalid | cond_others_all_nan| cond_equalzerp_others_null
            
            df_cleaned = df[~rows_to_drop]

            # 覆蓋儲存
            df_cleaned.to_csv(file_path, index=False)
            print(f"已處理：{filename}")
else:
    print("未選擇資料夾。")