#!/usr/bin/python
__autor__=''
from collections import deque
import string
import sys
import os
import cv2
import numpy as np
from PIL import Image
import glob


def main():
    splitmodel = int(input("請輸入圖片裁切模式 1:(平均分割),2:(迭代分割)) \n"))
    splitnum = int(input("請輸入裁切數量 \n"))

    
    # splitmodel = map(int,input("請輸入圖片裁切模式 1:(平均分割),2:(迭代分割)) \n").split())
    # splitnum = map(int,input("請輸入裁切數量 \n").split())

    # print('splitmodel= ' + splitmodel)
    # print('splitnum= ' + splitnum) 
    path = os.getcwd()
    pathsrc = os.getcwd()+'/Picture'
    pathlist = os.listdir(path)

    path_result = os.getcwd()+'/ResultCropImg'

    if not os.path.isdir(path_result):
        os.mkdir(path_result)

    Crop_jpg_files = glob.glob(path_result+"/*.jpg")


    for Crop_jpg_file in Crop_jpg_files:
     try:
          os.remove(Crop_jpg_file)
     except OSError as e:
        print(f"Error:{ e.strerror}")

    strcutpath = type(str(path_result))
#len =  len(pathsrc)

#print("pathsrc len = " +  len)

#for i in range(len(pathlist)):
    # a= open(os.path.join(path,i),'rb')
    #id = pathsrc[i].split(',')[0]
    #a_img = Image.open(path+'/'+pathsrc[i])
    a_img = Image.open(pathsrc+'/plot-mergy.jpg')
    w_len , h_len = a_img.size
   
    
    #(平均分割)
    if splitmodel == 1:
     strselectmode = "平均分割"
     id = 0
     div = int (splitnum // 2)
     weigth = int(w_len // div) # 寬長切輸入裁切數量的一半
     leigth = int(h_len // 2 ) #  高長切輸入2筆
     for j in range(2):  # 裁切高
         for k in range(div): #裁切寬
            box = ( weigth * k , leigth * j, weigth* (k+1),  leigth* (j+1))
            region = a_img.crop(box)
           # region.save(path_result+'/{}_{}{}.jpg'.format(id,j,k))
            region.save(path_result+'/{0:4d}.jpg'.format(id))

            id = id +1
    
    #2:(迭代分割)
    elif splitmodel == 2:
        strselectmode = "迭代分割"
        id = 0
        weigth_gap = int(w_len // splitnum) # 寬長輸入間距比例 為圖片寬/裁切數量
        leigth = int(h_len) # 高長切輸入原高度
        for k in range(splitnum): #裁切寬 ,高度不變 , 第一筆 N0 , 第二筆 N0+N1 ,類推
            box = ( weigth_gap * 0 , leigth*0, weigth_gap* (k+1),  leigth)
            region = a_img.crop(box)
           #region.save(path_result+'/{}_{}.jpg'.format(id,k))
            region.save(path_result+'/{0:4d}.jpg'.format(id))
            id = id +1
    
    print("裁切圖已經儲存> ResultCropImg 資料夾中, " + "選擇裁切模式為 (" + str(strselectmode)+ ")")
                   
# img = Image.open('plot-mergy.jpg')
# w , h = img.size
# if w % 2 == 0:
#     cut1 = int(w/2)  #對矩陣進行裁切時,數據類型應該是int
# else:
#     cut1 = int((w-1)/2)

# if h % 2 == 0:
#     cut2 = int(h/2)  #對h矩陣進行裁切時,數據類型應該是int
# else:
#      cut2 = int((h-1)/2)

# print('cut1= ',cut1 ,' cut2= ',cut2)

# img1_1 = img[0:cut1,0:cut2];img1_2 = img[cut1:w,0:cut2];img1_3 = img[0:cut1,cut2:h];
# img1_4 = img[cut1:w,cut2:h]

# img_list = { "img1_1" : img1_1 ,"img1_2" : img1_2,"img1_3" : img1_3,"img1_4" : img1_4}


# print('len(img_list)= ',len(img_list) )
# for r in range(len(img_list)):
#     if r == 0:
#         pass
# output = np.zeros((360,480,3), dtype='uint8')
# output[x:x+w, y:y+h]=crop_img



if __name__ == '__main__':
	 main()