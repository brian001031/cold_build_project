#!/usr/bin/python
import os, sys
import random
from datetime import datetime

output_file = None
today = None 
count_input = 0
totalcount = 0

# 生成亂數 0.000XX 浮點數格式 不為0 randint(1, 99) -> 1開始
def generate_random_numbers(count):
    result = []

    for _ in range(count):
        number = random.randint(1, 99)
        result.append(f"0.000{number:02d}")

    return result



# ==========================================
# 使用者輸入產生筆數
# ==========================================
while True:

    today = datetime.now().strftime("%Y%m%d_%H%M%S")

    try:
        count_input =int(input("請輸入要產生的亂數筆數(正整數)： " ))

        if count_input <= 0:
            print("錯誤：筆數必須大於 0，請重新輸入。")
            continue
        break

    except ValueError:
        print("錯誤：請輸入整數，例如 101 ")

#產生5位浮點數 
numbers = generate_random_numbers(count_input)

#定義檔案名稱
output_file = f"{today}_random_5float_list.txt"

# 寫入 TXT
# ==========================================
with open(output_file, "w", encoding="utf-8") as file:    
    for value in numbers:
        file.write(value + "\n")
        totalcount =  totalcount + 1


print(f"已產生 {totalcount} 筆亂數")
print(f"輸出檔案：{output_file}")