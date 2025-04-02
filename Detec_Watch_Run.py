import os
import time
import glob
from tkinter import TRUE
from watchdog.observers import Observer
from watchdog.events import FileSystemEventHandler
from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from selenium.common.exceptions import TimeoutException

# 監控的資料夾
WATCH_FOLDER = "Y:\\source_pfcc"
LAST_CSV_COUNT = len(glob.glob(os.path.join(WATCH_FOLDER, "*.csv")))


def wait_for_button(driver):
    try:
        # Wait until the button is present in the DOM and is visible (interactive)
        button = WebDriverWait(driver, 30).until(
            EC.visibility_of_element_located((By.ID, "MainContent_Button_Loop"))
        )
        print("autoLoop button found and ready for interaction!")
        return button
    except TimeoutException:
        print("Timed out waiting for the button to appear.")
        return None

def run_selenium_script():
    """當檔案數量增加時，執行 Selenium 點擊動作"""
    print("偵測到新 CSV 檔案，執行自動化程式...")

    options = webdriver.ChromeOptions()
   # options.add_argument("--headless")  # 無頭模式（不開啟瀏覽器）
    options.add_argument("--no-sandbox")
    options.add_argument("--disable-dev-shm-usage")

    driver = webdriver.Chrome(options=options)         
    driver.get("http://192.168.3.101:8001/")
        
    # 等到確定網頁開啟有找到autoLoop button 按鈕物件 ID (MainContent_Button_Loop)
    button = wait_for_button(driver)
	
    if button:
       #啟動Onclick 事件
       button.click()
       Begin_run = TRUE
       try:
           while Begin_run:
               try:				
                   Result_element = driver.find_element(By.ID, "MainContent_LResult")
                   if Result_element.text.strip() != "":
                       Begin_run = False  # 停止 while 迴圈
                   print(" AutoLoop complete, result updated.")
                   break
               except Exception as e:
                   # Handle exception if element is not found
                   print(f"Error occurred: {e}")
               time.sleep(1)  # Sleep to avoid overloading the system with constant checks 確保操作完成        
       finally:
           driver.quit()  # Close the browser tab and quit the driver session
           print("Browser session closed.")

class CSVFileHandler(FileSystemEventHandler):
    """監聽 CSV 檔案變化"""
    def on_created(self, event):
        global LAST_CSV_COUNT
        if event.is_directory:
            return

        # 只監控 CSV 檔案
        if event.src_path.endswith(".csv"):
            current_csv_count = len(glob.glob(os.path.join(WATCH_FOLDER, "*.csv")))
            
            if current_csv_count > LAST_CSV_COUNT:
                LAST_CSV_COUNT = current_csv_count  # 更新計數
                run_selenium_script()


if __name__ == "__main__":
    event_handler = CSVFileHandler()
    observer = Observer()
    observer.schedule(event_handler, WATCH_FOLDER, recursive=False)

    print(f"監控中... 監控資料夾: {WATCH_FOLDER}")
    
    try:
        observer.start()
        while True:
            time.sleep(10)  # 每 10 秒檢查一次
    except KeyboardInterrupt:
        observer.stop()

    observer.join()