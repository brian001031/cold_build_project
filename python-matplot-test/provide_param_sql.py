import matplotlib.pyplot as plt
import matplotlib.ticker as ticker
import matplotlib
import numpy as np
import pandas as pd
from PIL import Image
import cv2
import os
import random
import shutil
import glob

import re
import datetime
import time

from read_csvtest import main
path_sql_query = os.getcwd()+'/sql_query.txt'

# Python 3.x
param_range = range(3, 12) # 3到11的範圍

def generate_sql():
    sql_list = []
    sql_list.append("WITH cleaned AS (\n  SELECT * FROM mes.echk_batch\n  WHERE CAST(ID AS UNSIGNED) > 2\n)\n")

    for i in param_range:        
        param_col = f"PARAM{i:02d}"
        param_col_condition = f"CAST(PARAM{i:02d} AS DECIMAL(10,3))"
        label_min = f"bat_thickness_{i}_Min"
        label_max = f"bat_thickness_{i}_Max"
        condition = f"{param_col} REGEXP '^[0-9.]+$' AND {param_col_condition} NOT LIKE '0' AND {param_col_condition} IS NOT NULL AND {param_col_condition} > 0"

        sql_min = f"SELECT MIN(CAST({param_col} AS DECIMAL(10,3))) AS result, '{label_min}' AS type FROM cleaned WHERE {condition}"
        sql_max = f"SELECT MAX(CAST({param_col} AS DECIMAL(10,3))) AS result, '{label_max}' AS type FROM cleaned WHERE {condition}"

        sql_list.extend([sql_min, sql_max])


        # for stat in ['MIN', 'MAX']:
        #     label = f"{param}_{'Min' if stat == 'MIN' else 'Max'}"
        #     sql = (
        #         f"SELECT {stat}(CAST({param_col} AS DECIMAL(10,3))) AS result, '{label}' AS type FROM cleaned\n"
        #         f"WHERE {param_col} REGEXP '^[0-9.]+$' AND {param_col} NOT LIKE '0'"
        #     )
        #     sql_list.append(sql)
    
    union_all_query = "\nUNION ALL\n".join(sql_list[1:])
    full_sql =  f"""
        {sql_list[0]}
        {union_all_query}
        """.strip()

    return full_sql

def write_sqlquery_txt():
    sql = generate_sql()

    # 若檔案不存在，直接寫入
    if not os.path.exists(path_sql_query):
        print("檔案不存在，建立並寫入 SQL QUERY")
        with open(path_sql_query, "w", encoding="utf-8") as f:
            f.write(sql)
        return
    
    new_sql_lines = len(sql.splitlines())

    # 將 SQL QUERY 寫入檔案
    # 讀取每一行
    with open(path_sql_query, "r", encoding="utf-8") as f:
        lines = f.readlines()
    
    existing_line_count = len(lines)

    # #全文內容
    # with open(path_sql_query, "r", encoding="utf-8") as f:
    #     existing_sql = f.read()


    # lines = [line.strip() for line in lines]  # 去除每行的空白字元
    if not lines:
        print("檔案為空，將會寫入 SQL QUERY")
        # 將每一行的內容寫入到新的檔案中
        with open(path_sql_query, "w", encoding="utf-8") as f:
            f.write(sql)
    else:
        # 檢查 SQL QUERY 是否已存在       
        #if any(sql in line for line in lines):
        if new_sql_lines == existing_line_count:
            print("SQL QUERY 已存在，尾端繼續寫入!")
            with open(path_sql_query, "a", encoding="utf-8") as f:
                f.write("\n" + sql)
        else:
            print("SQL QUERY 不存在，將會覆蓋原有檔案!")
            # 覆蓋原有檔案
            with open(path_sql_query, "w", encoding="utf-8") as f:
                f.write(sql)
            
    print("SQL QUERY 寫入完成!")


if __name__ == "__main__":
    print("SQL QUERY 數量為 = " + str(len(param_range)))
    # 產生 SQL QUERY
    write_sqlquery_txt()
    # 印出 SQL QUERY
    print("SQL QUERY = " + generate_sql())