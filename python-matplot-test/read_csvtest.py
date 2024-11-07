from pathlib import Path
import unicodedata
import pandas as pd
import numpy as np
import os
import csv


datalist=list()
select_collist = list()


def main():
    path = os.getcwd()
    pathscsv = os.getcwd()+'/test7.csv'
    skip_num = 0
    # df = pd.read_csv(str(pathscsv))
    # add = str(df["數值"])
    #取行值
    # print(df.iloc[0]) 
    #取列值
    #print(df["數值"])
    txt_log = os.getcwd()+'/txt_record'
    log_txt_path = txt_log + "/selectColume.txt"
    if not os.path.isdir(txt_log):
        os.mkdir(txt_log)
    
    txtfile = Path(log_txt_path)
    txtfile.touch(exist_ok=True)


    file = open(pathscsv,"r",encoding="utf-8")
    reader = csv.reader(file)
    for r in reader:
        column = [row[3] for row in reader] #test '數值' 欄位擷取總列
        if str(column).isnumeric() == False:
            continue
        else:
           select_collist.append(column)
        #    skip_num =skip_num +1
    #print(column)
    select_collist.append(column)
 #   print(select_collist)
    file.close()
    
    #儲存column總list 寫入txt
    with open(txtfile, 'w') as f:
      f.write('value\n')
      f.write(str(select_collist))
    f.close()

    with open(txtfile) as f:
        for line in f:
          datalist.append(line)
    #      print(line.rstrip())
    f.close()

    print(datalist)

    print("CSV '數值' 欄位擷取總列完成已存取logtxt!")
    # with open(str(pathscsv),'r') as csvfile:
    #      reader = csv.DictReader(csvfile)
    #      for row in reader:
    #       print(row['數值']) 
        #  column = [row['數值'] for row in read]
        #  print(column)
    
    #datalist = np.array[column]
    #print("all column row['數值'] = " + datalist)

if __name__ == '__main__':
     main()