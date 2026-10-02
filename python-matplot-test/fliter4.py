#!/usr/bin/python
__autor__=''
from collections import deque
import concurrent.futures
import imageio.v2 as imageio
from pathlib import Path
from datetime import datetime, timedelta ,date
import subprocess
import random
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

accept_picture_type = ['.png','.jpg']

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

# 轉換原生圖像圖像顯示格式
def process_conv_image(file_name):
    # 取得副檔名（會自動帶點，例如：'.jpg'）
    #file_ext  = os.path.splitext(file_name)
    file_ext = Path(file_name).suffix
    image = Image.open(file_name)

    # 統一小寫比對 , 符合格式：才轉換為 RGB
    if file_ext.lower() in accept_picture_type:       
        return image.convert("RGB")
    
    return image

def create_video_thread_bindingFPS ( merge_picpath , final_videopath):

    print("準備執行 固定FPS = 10 禎 張數 video 轉換呈現!")

    # 设置動畫的帧率（例如，每秒10帧）  
    fps = 10 

    #當前Now日期
    today = datetime.now().strftime("%Y%m%d")

    # video file name  
    output_video = Path(final_videopath) / f"{today}_{fps}fps_dynamic.mp4"
    
    #獲取合併文件夹中所有圖像檔案
    image_getinfo = [
        f for f in Path(merge_picpath).iterdir()
        if f.is_file() and f.suffix.lower() in accept_picture_type
    ]

    # 排序文件路径，確認從數字由小到大排緒升冪排序        
    # int(x.stem) 已經取得無副檔名資訊
    image_getinfo.sort(key=lambda x: int(x.stem))


    #後續針對圖片做切換轉換格式(目前預設RGB優先)
    image_convfinal = [process_conv_image(f_img) for f_img in image_getinfo ]

    if not image_convfinal:
        raise RuntimeError("找不到合併merge圖片")


    # 第一張圖片決定影片尺寸
    first = cv2.imread(image_getinfo[0])
    height, width = first.shape[:2]

    # =========================
    # VideoWriter
    # =========================
    fourcc = cv2.VideoWriter_fourcc(*"mp4v")
    writer = cv2.VideoWriter(
        output_video,
        fourcc,
        fps, # 只用固定fps (預設值)        
        (width, height)
    )

    # =========================
    # play dynmaic video 
    # =========================
    for i, filename in enumerate(image_getinfo):
    
        img = cv2.imread(filename)

        if img is None:
            continue

        img = cv2.resize(img, (width, height))

        # 顯示目前 FPS
        text = f"Frame: {i + 1}/{len(image_getinfo)}  FPS: {fps}"

        # ---------------------------------
        #加入背景狀態文字陳述狀況
        # ---------------------------------

        cv2.putText(
                img,
                text,
                (30, 40),
                cv2.FONT_HERSHEY_SIMPLEX,
                0.8,
                (0, 255, 0),
                2
        )

        # 寫入影片存取(每張圖片依照偵數)
        writer.write(img)

        # 顯示
        cv2.imshow("Image Sequence", img)

        # q 離開
        if cv2.waitKey(int(1000 / fps)) & 0xFF == ord("q"):
            break
        
    writer.release()
    cv2.destroyAllWindows()
    print(f"完成動態影片測試：{output_video}")


def create_video_thread_dynmicFPS ( merge_picpath , final_videopath):

    # =====================================
    #  duration 最低高範圍設置
    # =====================================
    MIN_DURATION = 1       # ms
    MAX_DURATION = 250     # ms

    # 動態時間增量紀錄表單
    timestamps = []
    current_timestamp = 0

    #當前Now日期
    today = datetime.now().strftime("%Y%m%d")

    # video file name  
    output_video = Path(final_videopath) / f"{today}_fps_dynamic.mp4"

    # FFmpeg 中間檔案
    temp_dir = Path(final_videopath) / "_temp_dynamic"
    temp_dir.mkdir(parents=True, exist_ok=True)
   
    #獲取合併文件夹中所有圖像檔案
    image_getinfo = [
       f for f in Path(merge_picpath).iterdir()
       if f.is_file() and f.suffix.lower() in accept_picture_type
    ]

    if not image_getinfo:
        raise RuntimeError("找不到合併 merge 圖片")

    # 排序文件路径，確認從數字由小到大排緒升冪排序        
    #image_getinfo.sort(key=lambda x: int(os.path.splitext(x)[0]))  
    # int(x.stem) 已經取得無副檔名資訊
    image_getinfo.sort(key=lambda x: int(x.stem))

    # =====================================
    # 每張圖片產生隨機 duration 戳記ms
    # =====================================
    durations = [
        random.randint(MIN_DURATION, MAX_DURATION)
        for _ in image_getinfo
    ]

    # 動態current_timestamp 每張記錄累加
    for duration in durations:
        timestamps.append(current_timestamp)
        current_timestamp += duration

    total_duration = current_timestamp

    print(f"圖片數量：{len(image_getinfo)}")
    print(f"最小 duration：{MIN_DURATION} ms")
    print(f"最大 duration：{MAX_DURATION} ms")
    print(f"影片總長度：{total_duration} ms")
    print(f"影片總長度：{total_duration / 1000:.3f} sec")
    
    #後續針對圖片做切換轉換格式(目前預設RGB優先)
    #image_convfinal = [process_conv_image(f_img) for f_img in image_getinfo ]

    # 第一張圖片決定影片尺寸
    first = cv2.imread(image_getinfo[0])
    height, width = first.shape[:2]

    # =====================================
    # FFmpeg concat list
    # =====================================
    concat_file = temp_dir / "images.txt"
 


    # =====================================
    # OpenCV 處理圖片
    #
    # 注意：
    # 每一張處理完成後先輸出 PNG
    # FFmpeg 最後負責建立真正的時間軸
    # =====================================

    processed_images = []

    try:

        for i, filename in enumerate(image_getinfo):

            img = cv2.imread(str(filename))

            if img is None:
                print(f"⚠ 無法讀取圖片，跳過：{filename}")
                continue

            # =====================================
            # Resize
            # =====================================
            img = cv2.resize(img, (width, height))

            # ---------------------------------
            # 取得目前 Frame 的時間資訊
            # ---------------------------------
            play_timestamp = timestamps[i]
            record_duration = durations[i]

            # 動態 FPS
            current_fps = (
                1000.0 / record_duration
                if record_duration > 0
                else 0.0
            )

            # ---------------------------------
            # 顯示資訊
            # ---------------------------------

            text1 = (
                f"Frame: {i + 1}/{len(image_getinfo)}"
            )

            text2 = (
                f"Timestamp: {play_timestamp} ms"
            )

            text3 = (
                f"Duration: {record_duration} ms"
            )

            text4 = (
                f"Dynamic FPS: {current_fps:.2f}"
            )

            # ---------------------------------
            #加入背景狀態文字陳述狀況
            # ---------------------------------

            cv2.putText(
                    img,
                    text1,
                    (30, 40),
                    cv2.FONT_HERSHEY_SIMPLEX,
                    0.8,
                    (0, 255, 0),
                    2
            )

            cv2.putText(
                    img,
                    text2,
                    (30, 75),
                    cv2.FONT_HERSHEY_SIMPLEX,
                    0.8,
                    (0, 255, 255),
                    2
            )

            cv2.putText(
                    img,
                    text3,
                    (30, 110),
                    cv2.FONT_HERSHEY_SIMPLEX,
                    0.8,
                    (255, 255, 0),
                    2
            )

            cv2.putText(
                    img,
                    text4,
                    (30, 145),
                    cv2.FONT_HERSHEY_SIMPLEX,
                    0.8,
                    (0, 255, 255),
                    2
            )

            # =====================================
            # 暫存處理後圖片
            # =====================================
            temp_image = temp_dir / f"frame_{i:06d}.png"

            cv2.imwrite(
                str(temp_image),
                img
            )

            processed_images.append(temp_image)

            # =====================================
            # OpenCV 預覽
            #
            # 這裡使用真正 duration
            # =====================================
            cv2.imshow("Image Sequence", img)

            #等到最後一張延遲持度時間才做結束
            key = cv2.waitKey(
                max(1, record_duration)
            ) & 0xFF

            if key == ord("q"):
                print("使用者中止")
                return

        #先行確認IDE環境 是否有建立FFMPEG 應用程序
        FFMPEG_EXE = Path(r"C:\tools\ffmpeg\bin\ffmpeg.exe")

        if not FFMPEG_EXE.is_file():
            raise FileNotFoundError(
                f"找不到 FFmpeg：{FFMPEG_EXE}"
            )

        print("FFmpeg =", FFMPEG_EXE)


        subprocess.run(
            [str(FFMPEG_EXE), "-version"],
            check=True
        )

        # =====================================
        # 建立 FFmpeg concat file
        # concat demuxer 的 duration 單位是秒
        # =====================================
        with open(concat_file,"w",encoding="utf-8") as f:

            for image_path, duration in zip(
                processed_images,
                durations
            ):

             duration_sec = duration / 1000.0

             # FFmpeg concat file
             f.write(f"file '{image_path.resolve().as_posix()}'\n")
             f.write(f"duration {duration_sec:.6f}\n")

             # =====================================
            # FFmpeg concat demuxer
            #
            # 最後一張需要再次指定
            # =====================================
            if processed_images:

                last_image = (
                    processed_images[-1]
                    .resolve()
                    .as_posix()
                )

                f.write(
                    f"file '{last_image}'\n"
                ) 

        # =====================================
        # FFmpeg
        # =====================================
        # libx264 + yuv420p 要求影像寬高通常必須是偶數，因此 encoder 無法啟動
        ffmpeg_cmd = [
            str(FFMPEG_EXE),
            "-y",
            "-f",
            "concat",
            "-safe", "0",
            "-i", str(concat_file),
            #使用 pad，把奇數尺寸補成偶數
            "-vf", "pad=ceil(iw/2)*2:ceil(ih/2)*2",
            "-fps_mode", "vfr",

            # H.264            
            "-c:v",            
            "libx264",

            # 保持畫質
            "-preset",
            "medium",
            "-crf",
            "18",
            # Pixel format
            "-pix_fmt", "yuv420p",
            str(output_video)
        ]
 
        print("=====================================")
        print("開始 FFmpeg 編碼")
        print("=====================================")
        print(" ".join(f'"{x}"' if " " in x else x for x in ffmpeg_cmd))
        print("=====================================")


        result = subprocess.run(
            ffmpeg_cmd,
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            text=True,
            encoding="utf-8",
            errors="replace"
        )

        print("========== FFmpeg RESULT ==========")
        print("returncode =", result.returncode)
        print("stdout =")
        print(result.stdout)

        print("stderr =")
        print(result.stderr)

        print("===================================")

        # =====================================
        # FFmpeg 錯誤處理
        # =====================================
        if result.returncode != 0:            
            print(result.stderr)
            raise RuntimeError(
                "FFmpeg 建立 Dynamic MP4 失敗\n"
                + result.stderr
            )

        # =====================================
        # 完成
        # =====================================
        print("=====================================")
        print("Dynamic Video 完成")
        print("=====================================")
        print(f"Output：{output_video}")
        print(f"Total Duration：{total_duration / 1000:.3f} sec")
        print("=====================================")

    finally:
        #釋放占用記憶體配置(processed_images)
        cv2.destroyAllWindows()

        # =====================================
        # 清理暫存檔
        # =====================================
        for temp_file in processed_images:

            try:
                temp_file.unlink()
            except Exception:
                pass

        try:
             #移除寫入記錄檔案(concat_file)
            concat_file.unlink()
        except Exception:
            pass

        try:
             #移除暫存路徑(temp_dir)
            temp_dir.rmdir()
        except Exception:
            pass
                
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

    # 動態影像gif or mpeg4 儲存之路徑 
    path_video_build = os.getcwd()+'/video_build'

    #處理合併前暫時存取資料夾
    if not os.path.isdir(path_tmp):
        os.mkdir(path_tmp)

     #分割存取之資料夾
    if not os.path.isdir(path_result):
        os.mkdir(path_result)

    if not os.path.isdir(path_mergeall):
        os.mkdir(path_mergeall)

    if not os.path.isdir( path_video_build):
        os.mkdir( path_video_build)

    Crop_jpg_files = glob.glob(path_result+"/*.jpg")
    Crop_jpg_files2 = glob.glob(path_mergeall+"/*.jpg")
    temp_jpg_files = glob.glob(path_tmp+"/*.jpg")
    mpeg_video_files = glob.glob(path_video_build+"/*.*")
  

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


    for Crop_jpg_file in Crop_jpg_files2:
       try:
         os.remove(Crop_jpg_file)
       except OSError as e:
         print(f"Error:{ e.strerror}")

    # 刪除既有動態影片檔案(資料夾已經全部*.*不指定)
    try:
        for file_path in mpeg_video_files:
            if os.path.isfile(file_path):
                os.remove(file_path)
    except OSError as e:
        print('Delete Problem: ', e)


#len =  len(pathsrc)

#print("pathsrc len = " +  len)

#for i in range(len(pathlist)):
    # a= open(os.path.join(path,i),'rb')
    #id = pathsrc[i].split(',')[0]
    #a_img = Image.open(path+'/'+pathsrc[i])

    # 優先找固定檔名
    default_img = os.path.join(pathsrc, 'plot-mergy.jpg')
    
    if os.path.exists(default_img):
        img_path = default_img
      #  print(f'使用預設圖片: {img_path}')
    else:
        # 搜尋其他 Special*.jpg
        search_pattern = os.path.join(pathsrc, 'Special*.jpg')
        img_list = sorted(glob.glob(search_pattern))

        # 排除 plot-mergy.jpg 自己
        img_list = [
            x for x in img_list
            if os.path.basename(x) != 'plot-mergy.jpg'
        ]

        if len(img_list) == 0:
            raise Exception('找不到 plot-mergy 系列圖片')

        # 使用第一張
        img_path = img_list[0]
        print(f'找不到 plot-mergy.jpg，改用: {img_path}')

    # 開啟圖片
#   a_img = Image.open(pathsrc+'/plot-mergy.jpg')
    a_img = Image.open(img_path)
    
    # 取得圖片尺寸
    w_len , h_len = a_img.size

    print('此工作圖片_{}_寬 / _{}_高分別為:' , w_len , h_len)

    # 當遇到寬高比值小於1, (代表寬比高小),需要做圖片rotate轉向90度,原先指向相w,h對調,但無法真正將圖片對應運算
    #   功能	寫法	方向
    # 左轉90度	Image.ROTATE_90	逆時針
    # 右轉90度	Image.ROTATE_270	順時針
    # 180度	Image.ROTATE_180	上下顛倒
    if (w_len < h_len):
         print('圖片高度大於寬度,這邊做轉向90度(Width , Heigth )數值對調')
         a_img = a_img.transpose(Image.ROTATE_90)
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
                   # shutil.copyfile(pathsrc+'/plot-mergy.jpg',path_mergeall+'/{:3d}.jpg'.format(k))
                    shutil.copyfile(img_path,path_mergeall+'/{:3d}.jpg'.format(k))
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

    #接續執行影片製作
    #create_video_thread_bindingFPS( path_mergeall , path_video_build )
    create_video_thread_dynmicFPS( path_mergeall , path_video_build )
                   
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