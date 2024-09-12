#!/usr/bin/python
__autor__=''
from collections import deque
import string
import sys
import os
import PIL.Image
import cv2
import numpy as np
import PIL
from PIL import Image
import glob
import shutil

L_fx=[]
L_fy=[]

num_width_list=[]
num_length_list=[]

def create_long_image(image_folder, output_path, width=None, height=None):
   images = [Image.open(image_folder + '/' + img) for img in os.listdir(image_folder)]
   images = [img.resize((width, height)) for img in images]  # 将所有图像调整为同一大小
   widths, heights = zip(*(i.size for i in images))  # 获取所有图像的宽度和高度
   max_width = max(widths)  # 获取最大宽度
   total_height = sum(heights)  # 计算总高度
   new_img = Image.new('RGB', (max_width, total_height))  # 创建新图像
   y_offset = 0
   for img in images:  # 将所有图像粘贴到新图像上
     new_img.paste(img, (0, y_offset))
     y_offset += img.height
   new_img.save(output_path)  # 保存新图像

def bgTranstoWhite(img2,first_w ,img_W ,img_H):

   for yh in range(img_H):  # 起始圖片h位置裁切部分()因要全白部分,高度h不變動
     for xw in range(img_W -first_w):  # 起始圖片w位置變白部分
       dot = (xw,yh)
       color_d = img2.getpixel(dot)
       if (color_d[2]) > 100:
        color_d = [255,255,255,255]
        img2 = PIL.Image.format("")    
        img2.putpixel(dot,color_d)
        # xw = xw + first_w
        # dot = img2(xw,yh)
        # if (dot == np.array([0,0,0,0])).all():
        #   (xw,yh) = [255,255,255,255]
       else:
          L_fx.append(xw)
          L_fy.append(yh)
   box = (min(L_fx)-1 , min(L_fy)-1,max(L_fx)+1, max(L_fy)+1)
   img2 = img2.crop(box)
   return img2

def merge_picture(target_path,merge_path ,num_of_cols, num_of_rows):
    filename = file_name(target_path, ".jpg")

    if len(filename) == 0:
       print('此路徑_{}_無jpg檔案'.format(target_path))
       return
    
    shape = cv2.imread(filename[0], -1).shape  # 三通道的影像需把-1改成1,預設為4通道
    cols = shape[1]
    rows = shape[0]
    channels = shape[2]

    #這邊代表最後合併之寬高全分割圖片
    dst = np.zeros((rows * num_of_rows, cols * num_of_cols, channels), np.uint8) 
    for i in range(len(filename)):
     img = cv2.imread(filename[i])
     m, n = filename[i].split("\\")[-1].split(".")[0].split("_")
     cols_th = int(n)
     rows_th = int(m)
     roi = img[0:rows, 0:cols, :]
     dst[rows_th * rows:(rows_th + 1) * rows, cols_th * cols:(cols_th + 1) * cols, :] = roi

    cv2.imwrite(merge_path + "/mergeall.png", dst)

    #另外一種指定方式
    #  for pic in filename:
    #     num_width_list.append(int(pic.split(".")[0]))
    #     num_length_list.append(int((pic.split("_")[-1]).split(".")[0]))
     
    #  num_max_width = max(num_width_list)
    #  num_max_length = max(num_length_list)

    #  #預設拼接圖片
    #  splictpic = np.zeros((rows*num_max_width,cols*num_max_length,channels),np.uint8)
    #  for i in range(1,num_max_width+1):
    #   for j in range(1,num_max_length+1):
    #     imgpart = cv2.imread(target_path+'/{}_{}.jpg'.format(i,j))
    #     splictpic[rows*(i-1):rows*(i),cols*(j-1): cols*(j),:] = imgpart
    #  
    #  cv2.imwrite(merge_path + "/mergeall.png", dst)
     

# 遍历文件夹下的图片
def file_name(root_path, picturetype):
    filename = []
    for root, dirs, files in os.walk(root_path):
        for file in files:
            if os.path.splitext(file)[1] == picturetype:
                filename.append(os.path.join(root, file))
    return filename

def main():
    splitmodel = int(input("請輸入圖片裁切模式 1:(平均分割),2:(迭代分割)) \n"))
    splitnum = int(input("請輸入裁切數量 \n"))

    
    # splitmodel = map(int,input("請輸入圖片裁切模式 1:(平均分割),2:(迭代分割)) \n").split())
    # splitnum = map(int,input("請輸入裁切數量 \n").split())

    # print('splitmodel= ' + splitmodel)
    # print('splitnum= ' + splitnum) 
    path = os.getcwd()
    pathsrc = os.getcwd()+'/Picture'
  

    path_result = os.getcwd()+'/ResultCropImg'
    path_tmp = os.getcwd()+'/mergefix'

    #合併圖片儲存之路徑
    path_mergeall = os.getcwd()+'/mergeall'

    #處理合併前暫時存取資料夾
    if not os.path.isdir(path_tmp):
        os.mkdir(path_tmp)

     #分割存取之資料夾
    if not os.path.isdir(path_result):
        os.mkdir(path_result)

    if not os.path.isdir(path_mergeall):
        os.mkdir(path_mergeall)

    Crop_jpg_files = glob.glob(path_result+"/*.jpg")
    Crop_jpg_files2 = glob.glob(path_mergeall+"/*.jpg")
    temp_jpg_files = glob.glob(path_tmp+"/*.jpg")


    #重啟後先刪除指定路徑資料夾
    for Crop_jpg_file in Crop_jpg_files:
     try:
          os.remove(Crop_jpg_file)
     except OSError as e:
        print(f"Error:{ e.strerror}")

    for Crop_jpg_file in Crop_jpg_files2:
      try:
          os.remove(Crop_jpg_file)
      except OSError as e:
        print(f"Error:{ e.strerror}")

    # for temp_jpg_file in temp_jpg_files:
    #  try:
    #     os.remove(temp_jpg_file)
    #  except OSError as e:
    #     print(f"Error:{ e.strerror}")

    try:
        shutil.rmtree(path_tmp)
        if not os.path.isdir(path_tmp):
         os.mkdir(path_tmp)
    except OSError as e:
        print('Delete Problem: ', e)

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
            region.save(path_result+'/{}_{}{}.jpg'.format(id,j,k))
            id = id +1
    
    #2:(迭代分割)
    elif splitmodel == 2:
        strselectmode = "迭代分割"
        id = 0
        weigth_gap = int(w_len // splitnum) # 寬長輸入間距比例 為圖片寬/裁切數量
        leigth = int(h_len) # 高長切輸入原高度
        for k in range(splitnum): #裁切寬 ,高度不變 , 第一筆 N0 , 第二筆 N0+N1 ,類推
            if k < splitnum: # 最後一次為原圖origin size,因此不用執行 
                if k == splitnum - 1:
                    shutil.copyfile(pathsrc+'/plot-mergy.jpg',path_mergeall+'/{:3d}.jpg'.format(k))
                    continue  
                box = ( weigth_gap * 0 , leigth*0, weigth_gap* (k+1),  leigth)
                region = a_img.crop(box)
                region.save(path_result+'/{}_{}.jpg'.format(id,k))
                region.save(path_tmp+'/{}_{}.jpg'.format(id,k))


            # 將裁切剩餘部分全白 
           #bgTranstoWhite( a_img, weigth_gap, w_len , h_len )
              
                boxwhite = ( weigth_gap* (k+1) , leigth*0, w_len,  leigth)
                regionwhite = a_img.crop(boxwhite)
                wh_width , wh_lenght = regionwhite.size
                if wh_width > 0 and wh_lenght > 0:
                    imgwhite = Image.new('RGB',size=(wh_width,wh_lenght),color=(0,0,0))
                    # imgwhite.paste(a_img,(weigth_gap* (k+1),leigth*0),mask=a_img)
                    imgwhite.save(path_result+'/wb_{}_{}.jpg'.format(id,k))
                    imgwhite.save(path_tmp+'/{}_{}.jpg'.format(id,k+1))
                
            
            #將指定之圖片序號合併成圖(目前是將切割之對應序號兩張合併)
            filename = file_name(path_tmp, ".jpg")
            if len(filename) == 0:
               print('此路徑_{}_無jpg檔案'.format(path_tmp))
               return
            
            for i in range(len(filename)):
              if i == 0 :
                img_L = cv2.imread(filename[i])
              elif i == 1:
                img_R = cv2.imread(filename[i])  
            
            h0,w0 = img_L.shape[0],img_L.shape[1]
            h1,w1 = img_R.shape[0],img_R.shape[1]
            h = max(h0,h1)
            w = max(w0,w1)
            org_img= np.ones((h,w,3),dtype=np.uint8)*255
            trans_img= np.ones((h,w,3),dtype=np.uint8)*255

            org_img[:h0,:w0,:] = img_L[:,:,:]
            trans_img[:h1,:w1,:] = img_R[:,:,:]
            all_img = np.hstack((org_img[:,:w0,:],trans_img[:,:w1,:]))
            cv2.imwrite(path_mergeall+'/{:3d}.jpg'.format(id),all_img)
            # cv2.imshow("merge all ",all_img )
            # cv2.waitKey(0)

            id = id +1

            #將mergefix 資料夾中jpg檔刪除完畢
            # for temp_jpg in temp_jpg_files:
            #  try:
            #   os.remove(temp_jpg)
            #  except OSError as e:
            #   print(f"Error:{ e.strerror}")
            
            try:
             shutil.rmtree(path_tmp)
             if not os.path.isdir(path_tmp):
                os.mkdir(path_tmp)
            except OSError as e:
             print('Delete Problem: ', e)

             

            # shapes = cv2.imread(filename[0], -1).shape  # 三通道的影像需把-1改成1,預設為4通道
            # cols_set = shapes[1]
            # rows_set = shapes[0]
            # channels = shapes[2]

            # dst = np.zeros((rows_set * 2 , cols_set * int(splitnum // 2) , channels), np.uint8)
            # # dst = np.zeros((rows_all,cols_all,channels), np.uint8)

            # for i in range(len(filename)):
            #  img = cv2.imread(filename[i])
            #  m, n = filename[i].split("\\")[-1].split(".")[0].split("_")
            #  cols_th = int(n)
            #  rows_th = int(m)
            #  roi = img[0:rows_set, 0:cols_set, :]
            #  dst[rows_th * rows_set:(rows_th + 1) * rows_set, cols_th * cols_set:(cols_th + 1) * cols_set, :] = roi
         
            #  cv2.imwrite(path_mergeall + '/{}.jpg'.format(id), dst)
            
           
     
        
    # print("裁切圖已經儲存> ResultCropImg 資料夾中, " + "選擇裁切模式為 (" + str(strselectmode)+ ")")
    print("裁切圖儲存> ResultCropImg , " + " 合併圖儲存> mergeall ," +"各資料夾中")

                   
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