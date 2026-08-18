#!/usr/bin/python
import os, sys
import random
import unicodedata
from datetime import datetime

output_file = None
today = None 
over_check_number = False
check_from_direct = 1
count_input = 0
positive_number = 1
decimal_places = 0
totalcount = 0


# 目前會將浮點數強制轉為整數
def is_negtivenumber (input_value):
   try:
      # 已經是 int / float
        if isinstance(input_value, (int, float)):
            return int(input_value)

        # 字串
        input_value = input_value.strip()

        # 有小數點 → 先轉 float，再取整數
        if "." in input_value:
            return int(float(input_value))

        # 沒有小數點 → 直接轉 int
        return int(input_value)

   except (TypeError , ValueError):
      pass

   return input_value
   

# 生成亂數 xxx.xxx 浮點數格式 不為0 randint(1, 99) -> 1開始
def generate_random_numbers(count , positive_number , decimal_places , check_from_direct):
    result = []

    for _ in range(count):
        # 代表正整數 bit數自行定義 (目前因電容量分佈常態 50000mAH附近遊走) , 最大正整數位數的 55%
        ack_intenger_value = int( ((10 ** positive_number) - 1) * 0.55 )
        #ack_intenger_value = int( ((10 ** positive_number) - 1) * 1.0 )
        
        #代表亂數 = 10*number -1 
        max_random = (10 ** decimal_places) - 1

        max_integer = random.randint(check_from_direct, ack_intenger_value)

        decimal_number = random.randint(1, max_random)
        #原先固定浮點數第五位 ,以下可直接取商value為 = 0 , (0.浮點數顯示)
        #value = number / (10 ** decimal_places)
        # result.append(f"0.000{number:02d}") 
        #使用者自行定義 整數和浮點數位數
        value = max_integer + decimal_number  / (10 ** decimal_places)
        result.append(f"{value:.{decimal_places}f}")

        # 取小數點指定位數顯示 目前下列式ACIR常用顯示位元(小數第五位 4到5顯示值)
        # value = decimal_number
        # result.append(f"0.000{value:02d}")

        
    return result



# ==========================================
# 使用者輸入產生筆數
# ==========================================
while True:

    today = datetime.now().strftime("%Y%m%d_%H%M%S")

    try:
        count_input =int(input("請輸入要產生的亂數筆數(正整數)： " ))
        positive_number = int(input("請輸入正整數顯示多少位數,最多10位："))
        check_from_direct = int(input("輸入擷取正整數_亂數最小開始數字(整數),預設1："))      
        decimal_places = int(input("請輸入浮點數小數位數："))
        
        if count_input <= 0 :
            print("錯誤：筆數必須大於 0，請重新輸入。")
            continue

         # 驗證正整數位數
        if positive_number > 10:
            print("提示：正整數最多 10 位，已自動設定為 10 位。")
            positive_number = 10

        elif positive_number <= 0:
            print("錯誤：正整數位數不能小於 0，已設定為 1。")
            positive_number = 1
        
        check_from_direct = is_negtivenumber(check_from_direct)

        # 防止最小數字超過positive_number 位元數
       
        max_number = (10 ** positive_number) - 1
        over_check_number = check_from_direct > max_number

        #如果為0則需要重新制定
        if check_from_direct == 0:           
           print("錯誤：取得最小範圍數字為0，不可接收,請重新輸入。")
           continue
        elif over_check_number == True:
           print(f"錯誤：輸入最小值{check_from_direct}，不可接收,請重新輸入。")
           continue

        # 驗證小數位數
        if decimal_places <= 0:
            print("錯誤：小數位數不能小於 0，請重新輸入。")
            continue

        break

    except ValueError:
        print("錯誤：請輸入整數，例如 101 ")

#產生指定正整數及浮點數list 
numbers = generate_random_numbers( count_input , positive_number , decimal_places ,check_from_direct)

#定義檔案名稱
output_file = f"{today}_random_{positive_number}_negitive_{decimal_places}_float_list.txt"

# 寫入 TXT
# ==========================================
with open(output_file, "w", encoding="utf-8") as file:    
    for value in numbers:
        file.write(value + "\n")
        totalcount =  totalcount + 1


print(f"已產生 {totalcount} 筆亂數")
print(f"輸出檔案：{output_file}")