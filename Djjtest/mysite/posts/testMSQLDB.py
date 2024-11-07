import mysql.connector
import numpy as np
import string
import re

plist=[]

def IsFloatNum(str):
  s=str.split('.')
  if len(s)>2:
    return False
  else:
    for si in s:
     if not si.isdigit():
      return False
  return True

mydbconn = mysql.connector.connect(
      host = "localhost",
      user="root",
      password="K@admin123456",
      database="djdb"
    )

mycursor = mydbconn.cursor()

# 基本語法
sql = "select * from sid"
mycursor.execute(sql)

# myres 是個陣列，該陣列會讀入所有的資料 (fetchall())
myres = mycursor.fetchall()

num = 0

table = str.maketrans("","",",!")


for x in myres:
  plist.append(str(x))

#最後，透過迴圈，將資料一列一列印出來
for row in plist:
   print(row)

#print(datetime.strptime(dateString, dateFormatter))
#"Monday, May 13, 2020 20:01:56"
#dateFormatter = "%m, %B %d, %Y %H:%M:%S"
  
#測試將日期時間格式改為ex(July 31, 2024, 3:30 -> 2024-07-31 20:01:56)
# for row in plist:
#  sr = row.split(', ')
#  for j in sr:
#   if str(j).find("datetime.datetime") != -1:
#     str(j).replace('datetime.datetime','')
#     print(str[1:5])
# print('\n')


#  將其中索引字串做符號空白移除處理以下且判斷數字狀態(抓10筆預先測試)
# for j in plist:
#   num = num + 1
#   if num <=10:
#     for j in str.split(str(j)):
#      st = re.sub("([^\u0030-\u0039\u0041-\u007a])",'',str(j))
#      st = st.replace(' ','')
#      if IsFloatNum(st) == True:
#        print('"'+st+'"' , end='')
#      else:
#       print(st , end='')
#   else:
#     break
#   print('\n')
    
     

# for row in plist:
#   for i in range(len(row)):
#       if i < 8:
#         if isinstance(row[i],float) == True or isinstance(row[i],int) == True: 
#             print(str(row[i]) + ",",end='')
#         else:
#             print(row[i] + ",",end='')
#       else:
#           print(row[i])

#   for k in range(len(row)):
#      print(row[k],end='')
#   print('')   
     
     

  
