import os
import time
import glob
import shutil
from tkinter import TRUE
from datetime import datetime
from watchdog.observers import Observer
from watchdog.events import FileSystemEventHandler
from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from selenium.common.exceptions import TimeoutException
from selenium.webdriver.chrome.service import Service
import threading
import random
import socket
import requests
import discord
from discord.ext import commands
import keyboard
import pyautogui





# 設定要偵測的時間（12小時制 + AM/PM)
#target_time_str_notify = ["12:00:10 AM", "05:30:10 PM"]

target_time_str_notify = ["08:00 AM","12:00 PM", "06:10 PM","11:55 PM"]

already_triggered = set()  # 防止重複發送

#每個監控資料夾當天執行的檔案名稱做紀錄
SECI_CC_last_csv= []
SECI_PF_last_csv= []
CHROMA_CC_last_csv= []
CHROMA_PF_last_csv= []

#印出最後進度訊息
Message = []

# 儲存當天的日期
#last_checked_date = datetime.now().strftime("%Y%m%d")

#for 測試用
last_checked_date = datetime.now().strftime("%Y%m30")

#預設監聽port
listen_port = None  # 先宣告全域變數，避免未定義錯誤

#偵測監控路徑csv,刻意延遲待檔案一定傳輸數量
delay_put_csv_time=3

# 設定期望的 CSV 檔案數量
expected_csv_count = 2   

#最少執行數據轉化的 CSV 檔案數量
limit_csv_runcount = 1

#當尋找當天日期csv格式檔案時，會有延遲時間,這邊容許的搜尋次數
trytime_max = 2

#決定執行selenium 執行續的序號
thread_num = 0

# 鎖來保證每次只會有一個 Selenium 實例執行
selenium_lock = threading.Lock()

# Global driver variable
driver = None  # Initialize the driver as None initially

# Define the global LAST_CSV_COUNT
LAST_CSV_COUNT = {}

#選用 ChromeDriver 路徑
chromedriver_path = "C:/drivers/win64/chromedriver.exe"

# 設定 Discord Bot 的 token 和 電化學PFCC分析數據 channel ID
TOKEN = 'MTI5MzM5Njg1MTExNDExOTE5OA.GsFrkY.YO_IBAIVji9DE86mgHygRYZq5Tl7pged6raBNs'
CHANNEL_ID = 1359443169414090862  

csv_filename = []  # 用來儲存所有 CSV 檔案名稱的列表

# 機器人設定
intents = discord.Intents.default()
bot = commands.Bot(command_prefix='!', intents=intents)

# 監控的資料夾
DETEC_VENDER_FOLDERS = [
    r"Y:\\TrayCellData\\R_bak_HTBI\\CC",
    r"Y:\\TrayCellData\\R_bak_HTBI\\PF",
    r"Y:\\TrayCellData\\R_bak_HTBI\\Chroma\\CC",
    r"Y:\\TrayCellData\\R_bak_HTBI\\Chroma\\PF",
]


# 實際運行分析資料夾
RUNTIME_ACTION_FOLDERS = [
    "C:\\copy_temp\\SECI_Source_CC",
    "C:\\copy_temp\\SECI_Source_PF",
    "C:\\copy_temp\\Chroma_Source_CC",
    "C:\\copy_temp\\Chroma_Source_PF",
]


# 設定每個資料夾的組態對應
#佈署端環境設定
FOLDER_CONFIG = {
     "SECI_CC": "http://192.168.3.101:8002/?side=seci_cc",
     "SECI_PF": "http://192.168.3.101:8002/?side=seci_pf", 
     "CHROMA_CC": "http://192.168.3.101:8002/?side=chroma_cc",
     "CHROMA_PF": "http://192.168.3.101:8002/?side=chroma_pf",
 }

#開發本機端環境設定
#FOLDER_CONFIG = {
#    "SECI_CC":   "https://localhost:44374/?side=seci_cc",
#    "SECI_PF":   "https://localhost:44374/?side=seci_pf",
#    "CHROMA_CC": "https://localhost:44374/?side=chroma_cc", 
#    "CHROMA_PF": "https://localhost:44374/?side=chroma_pf",
#  }
#

def get_thread_num(folder_path):
    """根據來源資料夾決定對應的執行緒號碼"""
    if "CC" in folder_path and "Chroma" not in folder_path:
        return 1  # SECI_Source_CC
    elif "PF" in folder_path and "Chroma" not in folder_path:
        return 2  # SECI_Source_PF
    elif "Chroma" in folder_path and "CC" in folder_path:
        return 3  # Chroma_Source_CC
    elif "Chroma" in folder_path and "PF" in folder_path:
        return 4  # Chroma_Source_PF
    else:
        raise ValueError(f"無法匹配執行緒號碼，來源資料夾: {folder_path}")

def create_today_folder_path(productcount,folder_path):
    """根據當前日期生成資料夾名稱"""
    current_year = datetime.now().strftime("%Y")
    today = datetime.now().strftime("%Y%m%d")
    year_folder_path = os.path.join(folder_path, current_year)
    target_folder = os.path.join(year_folder_path, today)
    #target_folder = os.path.join(folder_path, f"{today}-{productcount}pcs")

    print(f"當前日期時間為：{datetime.now()}")
    
    global last_checked_date

    # 比较日期是否发生变化
    if last_checked_date  != today:        
        last_checked_date = today
        print(f"目前偵測確定日期為 = " , last_checked_date)
        #清除觸發紀錄set和csv_filename 列表
        already_triggered.clear()
        SECI_CC_last_csv.clear()
        SECI_PF_last_csv.clear()
        CHROMA_CC_last_csv.clear()
        CHROMA_PF_last_csv.clear()
        print(f"日期已變更，更新為: {today} ,清除當天SECI,CHROMA回報紀錄儲存檔案名稱列表")

    # 如果當天日期資料夾不存在，則創建它
    #if not os.path.exists(target_folder):
    #    print(f"創建資料夾: {target_folder}")
    #    os.makedirs(target_folder)
    
    # 如果當年資料夾不存在，則創建它    
    if not os.path.exists(year_folder_path):
        print(f"創建資料夾: {year_folder_path}")
        os.makedirs(year_folder_path)
    
    return year_folder_path

def determine_report_PFCC_CSVFileName(csvfile):
    if csvfile.startswith("H000"):
       SECI_CC_last_csv.append(csvfile)  # 儲存SECI_CC檔案名稱
    elif csvfile.startswith("K000"):
       SECI_PF_last_csv.append(csvfile)  # 儲存SECI_PF檔案名稱
    elif csvfile.startswith("CC000"):
       CHROMA_CC_last_csv.append(csvfile)  # 儲存CHROMA_CC檔案名稱
    elif csvfile.startswith("PF000"):
       CHROMA_PF_last_csv.append(csvfile) # 儲存CHROMA_PF檔案名稱
    else:
       print(f"檔案名稱不符合預期: {csvfile}")

# 根據來源資料夾決定對應的目標資料夾
def get_runtime_target_folder(folder_path):
    if "CC" in folder_path and "Chroma" not in folder_path:      
        return RUNTIME_ACTION_FOLDERS[0]  # SECI_Source_CC
    elif "PF" in folder_path and "Chroma" not in folder_path:       
        return RUNTIME_ACTION_FOLDERS[1]  # SECI_Source_PF
    elif "Chroma" in folder_path and "CC" in folder_path:       
        return RUNTIME_ACTION_FOLDERS[2]  # Chroma_Source_CC
    elif "Chroma" in folder_path and "PF" in folder_path:        
        return RUNTIME_ACTION_FOLDERS[3]  # Chroma_Source_PF
    else:
        raise ValueError(f"無法匹配目標資料夾對應，來源資料夾: {folder_path}")

# 用來移動檔案的函式
def move_files_to_backup_and_watch_folder( currentday_count,folder_path):
    today_save_folder = create_today_folder_path(currentday_count,folder_path)    
    today_date_str = datetime.now().strftime("%Y%m%d")
    csv_files_today = glob.glob(os.path.join(folder_path, f"*{today_date_str}*.csv"))

   # print(f"當前資料夾 {folder_path} 中的 CSV 檔案: {csv_files_today}")

    if csv_files_today:            
        # 取得實際運行路徑資料夾並設定當前執行緒號碼
        target_run_path = get_runtime_target_folder(folder_path)
        csv_filename.clear()  # 清空列表以便儲存新的檔案名稱

        for csv_origin_analysis in csv_files_today:
            try:
                # 複製檔案到備份當天日期資料夾
                shutil.copy(csv_origin_analysis, today_save_folder)
                print(f"檔案 {csv_origin_analysis} 複製到資料夾 {today_save_folder}", flush=True)

                # 複製檔案到實際運行路徑資料夾
                shutil.copy(csv_origin_analysis, target_run_path)
             #   print(f"檔案 {csv_origin_analysis} 複製到 {target_run_path}")
                
                # 將檔案名稱加入列表
                csv_filename.append(os.path.basename(csv_origin_analysis))  
                # 判斷檔案名稱並儲存到對應的列表中
                determine_report_PFCC_CSVFileName(os.path.basename(csv_origin_analysis))
                # 複製完成後刪除原始檔案
                os.remove(csv_origin_analysis)
             #   print(f"檔案 {csv_origin_analysis} 已刪除")
            except Exception as e:
                print(f"處理檔案 {folder_path} 時出現錯誤: {e}")
    else:
        print("當天沒有新的 CSV 檔案需要移動")

# 用來檢查當天日期的 CSV 檔案
def check_csv_file_exists(folder_name):
    today_date = datetime.today().strftime("%Y%m%d")  # 當天的日期，例如 "20250408"
    
    # 檢查指定資料夾下是否有符合條件的 CSV 檔案
    for root, dirs, files in os.walk(folder_name):
        for file in files:
            if file.endswith(".csv") and today_date in file:
                print(f"檔案 {file} 在資料夾 {folder_name} 中找到，符合當天日期。")
                return True  # 找到符合條件的檔案，返回 True
    return False  # 沒有符合條件的檔案

def monitor_csv_files():
    # 需要偵測符合條件的資料夾
    threads = []

    # 檢查每個資料夾中的 CSV 檔案
    for folder_path in DETEC_VENDER_FOLDERS:
        # 根據資料夾來選擇對應的 FOLDER_CONFIG URL
        for key, url in FOLDER_CONFIG.items():
            if check_csv_file_exists(folder_path):  # 檢查資料夾中是否有符合當天日期的 CSV 檔案
                # 如果條件成立，則啟動新的執行緒
                thread = threading.Thread(target=run_selenium_script_mulitThread, args=(url, key))
                threads.append(thread)
                thread.start()

    # 等待所有執行緒結束
    for thread in threads:
        thread.join()

def wait_for_button(driver):
    try:
        # Wait until the button is present in the DOM and is visible (interactive)
        button = WebDriverWait(driver, 30).until(
            EC.visibility_of_element_located((By.ID, "MainContent_Button_Loop"))
        )
       # print("autoLoop button found and ready for interaction!")
        return button
    except TimeoutException:
        print("Timed out waiting for the button to appear.")
        return None
    except Exception as e:
        print(f"Error waiting for button: {e}")
        return None
def is_browser_alive(driver):
    try:
        # 使用多種方法檢查會話是否有效
        driver.title  # 嘗試獲取標題
        driver.session_id  # 檢查 session_id
        driver.capabilities  # 檢查 capabilities
       # driver.execute_script("return document.readyState")  # 執行 JavaScript 測試
       # driver.current_url  # 嘗試獲取當前 URL
        return True  # 若所有檢查都通過，表示會話有效
    except Exception:
        return False  # 若任一檢查失敗，表示會話無效

# 假設這是創建瀏覽器會話的函數
def start_new_selenium_session():  
    global listen_port    
    free_port = find_free_port()
    #print(f"free_port ={free_port}")
    if is_port_alive():        
        print(f"服務活著，可以接著呼叫 driver.get(): port {free_port}")
    else:
        #print(f"服務沒有活著: port {free_port}")
        listen_port =8002
        if is_port_alive():
            print(f"服務活著，可以接著呼叫 driver.get(): port {listen_port}")
        else:
            print(f"服務再次沒有活著: port {listen_port}")
        
    options = webdriver.ChromeOptions()
    # options.add_argument("--headless")  # 無頭模式（不開啟瀏覽器）
    options.add_argument("--no-sandbox")
    options.add_argument("--disable-dev-shm-usage")
    options.add_argument('--disable-gpu')  # 禁用 GPU 加速
    options.add_argument(f'--remote-debugging-port={free_port}')  # 指定遠端調試端口
    # 創建 WebDriver 服務
    service = Service(executable_path=chromedriver_path)
    # 使用預設的 chromedriver 路徑
    #service = Service()
   
    driver = webdriver.Chrome(service=service , options=options)     
    return driver

# 用來啟動多執行緒的範例
def start_thread_for_task(thread_num):
    threading.Thread(target=run_selenium_script, args=(thread_num,)).start()

# 模擬多執行緒執行 Selenium 腳本
def start_threads():
    threads = []
    for i in range(1, len(FOLDER_CONFIG) + 1):  # 根據 FOLDER_CONFIG 的數量創建多個執行緒
        thread = threading.Thread(target=run_selenium_script, args=(i,))
        threads.append(thread)
        thread.start()

    # 等待所有執行緒結束
    for thread in threads:
        thread.join()

def find_free_port():
    """找到可用的端口"""    
    global listen_port
    while True:
        port = random.randint(1024, 65535)  # 選擇一個隨機端口
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
            result = s.connect_ex(('192.168.3.101', port))
            if result != 0:
                listen_port = port
                #print(f"✅ find_free_port: listen_port 設為 {listen_port}")
                return port

def is_port_alive(host='192.168.3.101',timeout=20):
    """檢查指定 host:port 是否有服務在聽且回應"""
    global listen_port
    if listen_port is None:
        #print("⚠️ listen_port 尚未設定，請先呼叫 find_free_port()")
        return False
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.settimeout(timeout)
        try:
           # print(f"🔍 is_port_alive: 檢查 {host}:{listen_port}")
            s.connect((host, listen_port))
            return True
        except (socket.timeout, ConnectionRefusedError, OSError):
           # print(f"❌ 連線失敗 {host}:{listen_port}")
            return False

def notify_discord_webhook(msg):
		url = 'https://discord.com/api/webhooks/1359443349664301066/waIqQsAnT-6yLNWG6tsOcgWoA4CtUv4LPijXvvG5nfY6nS6nq5yjfrFiBEP8kuaR4srC'
		headers = {"Content-Type": "application/json"}
		data = {"content": msg, "username": "PFCC_Monitor"}
		res = requests.post(url, headers = headers, json = data) 
		if res.status_code in (200, 204):
				print(f"已成功將電化學分析結果回傳到Discord通報群組: {res.text}")
		else:
				print(f"Request failed with response: {res.status_code}-{res.text}")

def DetecProcess_reportcsv_timing(curenttime_status):
    for target in target_time_str_notify:        
        # 檢查是否達到指定通報時間
        if curenttime_status == target and target not in already_triggered:
            print(f"已達到指定通報時間: {target}，發送通知")
            Message.clear()

            if SECI_CC_last_csv:              
              Message.append(f"SECI_CC 最後處理分析檔案: {SECI_CC_last_csv[-1]}, flush=True) ->完成SECI CC(分容) 分析數量 : {len(SECI_CC_last_csv)}")
            else:
              Message.append("SECI_CC 清單為空")

            if SECI_PF_last_csv:                
                Message.append(f"SECI_PF 最後處理分析檔案: {SECI_PF_last_csv[-1]}, flush=True) ->完成SECI PF(化成) 分析數量 : {len(SECI_PF_last_csv)}")
            else:
                Message.append("SECI_PF 清單為空")

            if CHROMA_CC_last_csv:
                Message.append(f"CHROMA_CC 最後處理分析檔案: {CHROMA_CC_last_csv[-1]}, flush=True) ->完成CHROMA CC(分容) 分析數量 : {len(CHROMA_CC_last_csv)}")
            else:
                Message.append("CHROMA_CC 清單為空")

            if CHROMA_PF_last_csv:                
                Message.append(f"CHROMA_PF 最後處理分析檔案: {CHROMA_PF_last_csv[-1]}, flush=True) ->完成CHROMA PF(化成) 分析數量 : {len(CHROMA_PF_last_csv)}")
            else:
                Message.append("CHROMA_PF 清單為空")

            # 發送訊息到 Discord
            notify_discord_webhook(f"PFCC進度通報: {current_time} \n" + "\n".join(Message))                                                        
            print(f"已發送通知到 Discord: {current_time}", flush=True)
            already_triggered.add(target)

def run_selenium_script(autoLoop_thread_num):
  #當需要lock 單一執行續,需要使用driver 全域變數 , 並在外部宣告 driver = None -----start---
  global driver  # Access the global driver variable
  #---------------end-----------------------------------------------------------------

  #當需要使用多執行續,不使用driver 全域變數, 直接在函式內部宣告 driver = None
  #driver = None

  with selenium_lock:    
    if driver is None or not is_browser_alive(driver):
       print("瀏覽器會話無效，重新啟動瀏覽器...", flush=True)
       driver = start_new_selenium_session()
    else:
      """當檔案數量增加時，執行 Selenium 點擊動作"""
      print("偵測到新 CSV 檔案，執行自動化程式...")

    # 根據 thread_num 來選擇對應的 URL
    folder_names = list(FOLDER_CONFIG.keys())
    if autoLoop_thread_num <= len(folder_names):  # 確保 thread_num 在範圍內                   
        url = FOLDER_CONFIG[folder_names[autoLoop_thread_num - 1]]  # 因為 thread_num 從 1 開始，所以對應的索引是 thread_num - 1
        print(f"開始執行運行資料夾: {RUNTIME_ACTION_FOLDERS[autoLoop_thread_num - 1]}", flush=True) 
        filterpath = RUNTIME_ACTION_FOLDERS[autoLoop_thread_num - 1]  # 對應的資料夾路徑
        side_part = filterpath.split("\\")[-1].replace("_Source_", "_")  # 取出最後一個元素並替換 '_Source_' 為 '_'
        print('分析站為:', side_part, flush=True)

        if driver is None or not is_browser_alive(driver):   
         #  print("啟動新的瀏覽器會話Session執行前準備作業")
           free_port = find_free_port()
           #print(f"free_port ={free_port}")
           # 使用 ChromeOptions 設定瀏覽器選項 
           if is_port_alive():
                print(f"服務活著，可以接著呼叫 driver.get(): port {listen_port}")
           
           options = webdriver.ChromeOptions()
            # options.add_argument("--headless")  # 無頭模式（不開啟瀏覽器）
           options.add_argument("--no-sandbox")
           options.add_argument("--disable-dev-shm-usage")
           options.add_argument('--disable-gpu')  # 禁用 GPU 加速
           options.add_argument(f'--remote-debugging-port={free_port}')  # 指定遠端調試端口
           # 創建 WebDriver 服務
           service = Service(executable_path=chromedriver_path)
           # 使用預設的 chromedriver 路徑
           #service = Service()
           driver = webdriver.Chrome(service=service , options=options) 

        #driver.get("http://192.168.3.101:8002/")
        driver.get(url)  # 開啟對應的 URL
        driver.implicitly_wait(5)  # 等待網頁載入完成

       # print(f"啟動分析站點URL: {url}")
    else:
        print(f"無效的thread_num: {autoLoop_thread_num}，請提供有效的值")
        return

    # 等到確定網頁開啟有找到autoLoop button 按鈕物件 ID (MainContent_Button_Loop)
    button = wait_for_button(driver)

    if button:
       #啟動Onclick 事件
       button.click()
       Begin_run = TRUE
       run_count = 0
       print("AutoLoop button already find and clicked!")
       # 等待網頁載入完成，並確認 autoLoop 按鈕已經被點擊
       try:
           while Begin_run:
               try:				
                   Result_element = driver.find_element(By.ID, "MainContent_LResult")
                   run_count += 1
                   if( run_count % 10 == 0):  # 每10秒檢查一次
                      print("still in progress ongoing...")
                   #if Result_element.text.strip() != "":
                   if "合併" in Result_element.text.strip(): #代表有跑完自動化偵測之csv檔案
                       if "異常" in Result_element.text.strip(): 
                           notify_discord_webhook(f"分析站:{side_part} \n{Result_element.text.strip()} \n NG存放區:pf-cc-testNG")
                           print(" AutoLoop complete, have 'NG' in this process!")
                       else:
                      #     notify_discord_webhook(f"分析站:{side_part} {Result_element.text.strip()} \n 檔案列: {csv_filename}")
                           print(" AutoLoop complete, result updated @.@", flush=True)
                       Begin_run = False  # 停止 while 迴圈                       
                   break
               except Exception as e:
                   # Handle exception if element is not found
                   print(f"Error occurred: {e}")
               time.sleep(1)  # Sleep to avoid overloading the system with constant checks 確保操作完成        
       finally:
           driver.quit()  # Close the browser tab and quit the driver session
           print("Browser session closed.")

def run_selenium_script_mulitThread(url, autoLoop_thread_num):
    driver = None
    
    print(f"開始執行資料夾: {autoLoop_thread_num}，對應的 URL: {url}")

    # 每個執行緒都創建自己的 Selenium 實例
    options = webdriver.ChromeOptions()
    # options.add_argument("--headless")  # 無頭模式（不開啟瀏覽器
    options.add_argument("--no-sandbox")
    options.add_argument("--disable-dev-shm-usage")
    driver = webdriver.Chrome(options=options)

    # 開啟對應的 URL
    driver.get(url)
    driver.implicitly_wait(5)  # 等待網頁載入完成
    print(f"啟動分析站點URL: {url}")

    # 等到確定網頁開啟有找到autoLoop button 按鈕物件 ID (MainContent_Button_Loop)
    button = wait_for_button(driver)
    if button:
        # 啟動Onclick 事件
        button.click()
        begin_run = True
        run_count = 0
        print("AutoLoop button already find and clicked!")
        
        # 等待網頁載入完成，並確認 autoLoop 按鈕已經被點擊
        try:
            while begin_run:
                try:
                    result_element = driver.find_element(By.ID, "MainContent_LResult")
                    run_count += 1
                    if run_count % 10 == 0:  # 每10秒檢查一次
                        print("still in progress ongoing...")
                    if "合併" in result_element.text.strip():  # 代表有跑完自動化偵測之csv檔案
                        begin_run = False  # 停止 while 迴圈
                        print("AutoLoop complete, result updated.")
                    break
                except Exception as e:
                    print(f"Error occurred: {e}")
                time.sleep(1)  # Sleep to avoid overloading the system with constant checks
        finally:
            driver.quit()  # 關閉瀏覽器會話
            print("Browser session closed.")

class CSVFileHandler(FileSystemEventHandler):
    """監聽 CSV 檔案變化"""
    def on_created(self, event):       
        global LAST_CSV_COUNT
        if event.is_directory:
            return
                
        # 只監控當前日期之 CSV 檔案
        if event.src_path.endswith(".csv"): 
            trytime = 0        
          #  print(f"偵測到新檔案: {event.src_path}")
            # 取得當前檔案的日期               
            detec_folder_path = os.path.dirname(event.src_path)                    
            today_date_str = datetime.now().strftime("%Y%m%d")

            # print("偵測路徑為:", detec_folder_path)
            # print(f"偵測到新檔案日期為: {today_date_str}")
            
            current_csv_count = len(glob.glob(os.path.join(detec_folder_path, f"*{today_date_str}*.csv")))

            # 等待檔案數量達到預期數量
            while current_csv_count < expected_csv_count and trytime < trytime_max:
                trytime += 1
                # 延遲 7 秒鐘再檢查檔案
                time.sleep(delay_put_csv_time)                
                current_csv_count = len(glob.glob(os.path.join(detec_folder_path, f"*{today_date_str}*.csv")))
                if trytime ==  trytime_max-1:
                 print(f"總等待時間(秒): {delay_put_csv_time*trytime_max} 最後當前檔案數量: {current_csv_count}, 預期檔案數量: {expected_csv_count}, ", flush=True )
         
            # print(f"準備執行分析 {today_date_str} CSV 檔案數量為: {current_csv_count}")

            # print(f"LAST_CSV_COUNT 目前為 : {LAST_CSV_COUNT}")
            # print(f"LAST_CSV_COUNT[detec_folder_path] 目前為 : {LAST_CSV_COUNT.get(detec_folder_path, 0)}")
            # print(f"當前檔案數量: {current_csv_count}")

            # 確保每個資料夾都會有各自的計數器
            if detec_folder_path not in LAST_CSV_COUNT or current_csv_count == 0 or LAST_CSV_COUNT.get(detec_folder_path, 0) > 0:
              #  print(f"初始化計數器: {detec_folder_path}")
                # 初始化計數器
                LAST_CSV_COUNT[detec_folder_path] = 0
                
            if current_csv_count > LAST_CSV_COUNT[detec_folder_path]:
                print(f"檔案數量變化: {LAST_CSV_COUNT[detec_folder_path]} -> {current_csv_count}", flush=True) 
                # 更新計數器
                LAST_CSV_COUNT[detec_folder_path] = current_csv_count  # 更新該資料夾的計數,意旨目前機器已產生當天的數據電化學數據csv
                
                # Check if the current count has reached the expected number of CSV files for the day
                if current_csv_count >= limit_csv_runcount:     #至少執行一筆           
                    # 當檔案數量變動時，先備份當天的 CSV 檔案並將當日csv檔案移動到實際運行資料夾
                    move_files_to_backup_and_watch_folder(current_csv_count ,detec_folder_path)
                    # 更新執行緒號碼    
                    global thread_num
                    thread_num = get_thread_num(detec_folder_path)
                    # 確保在檔案全部收集後才執行 Selenium 腳本                
                    run_selenium_script(thread_num)

if __name__ == "__main__":
    event_handler = CSVFileHandler()
    observer = Observer()

    # 對目前機器原始數據之(SECI,CHROMA)設置監控
    for folder in DETEC_VENDER_FOLDERS:
        observer.schedule(event_handler, folder, recursive=False)

    # folder1 = DETEC_VENDER_FOLDERS[0]  # 監控第一個資料夾

    #folder2 = DETEC_VENDER_FOLDERS[1]  # 監控第二個資料夾
    # observer.schedule(event_handler, folder1, recursive=False)
    # observer.schedule(event_handler, folder2, recursive=False)
    #print(f"監控中... 監控資料夾: {folder1} and {folder2} ")

    # 多個執行緒來運行
    # keys = list(FOLDER_CONFIG.keys())
    # for i in range(len(keys)):
    #     print(f"啟動執行緒 {i} 用於link URL: {FOLDER_CONFIG[keys[i]]}")
    #     start_thread_for_task(i)

    # 執行多執行緒
   # start_threads()

   # 啟動檔案監控(多執行緒)
   # monitor_csv_files() 
     
    print(f"監控中... 監控資料夾: {DETEC_VENDER_FOLDERS} ")
    
    try:
        observer.start()
        counter = 0
        while True:            
           # print("監控中... 系統運作正常，正在檢查檔案變更中...")
           # 取得現在的時間，格式為 12小時制 + AM/PM
            current_time = datetime.now().strftime("%I:%M:%S %p")
            current_time_minute = datetime.now().strftime("%I:%M %p")

           # print(f"當前時間(時分): {current_time_minute}", flush=True)

            DetecProcess_reportcsv_timing(current_time_minute)  # 檢查是否達到指定通報時間
            time.sleep(10)  # 每 10 秒檢查一次
            #counter += 1
            #print(f"系統正常監控中... 經過 {counter * 10} 秒", flush=True)
            counter += 1
            if( (counter *10) % 60 ==0):
              print(f"系統正常監控中... 經過 { (counter * 10)//60 } 分鐘", flush=True)
    except KeyboardInterrupt:
        observer.stop()
    
    observer.join()