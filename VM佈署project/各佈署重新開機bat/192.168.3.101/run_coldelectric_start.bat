@echo off
rem 開始執行

:loop
echo coldelectric回報系統正在重啟執行.....

rem 確認 Python 是否已安裝
where python >nul 2>&1 

if errorlevel 1 ( 
	echo. echo [錯誤] 找不到 Python！ 
	echo 請確認 Python 是否已安裝，以及是否已加入 PATH 環境變數。 
	echo. pause exit 
	/b 1 
) 

rem 顯示 Python 版本 echo [OK] 找到 Python： 
python --version


echo 等待 1秒鐘後 後再執行...
timeout /t 1 /nobreak

rem 在同一命令視窗中執行 Python 腳本 Detec_Watch_AutoRun_Release.py 並保留命令視窗
start cmd /K "python.exe C:\Detec_Watch_AutoRun_Release.py"

if errorlevel 1 (
    echo Detec_Watch_AutoRun_Release.py 執行錯誤，結束執行
    exit /b 1
)


rem 開啟並執行第一個專案 (innerWeb)
echo 開啟內部報修及銷庫存網頁專案...
cd C:\Project_official\COLD_frontweb
start npm run start

echo 等待 2秒鐘後 後再執行...
timeout /t 2 /nobreak

rem 開啟並執行第二個專案 (innerbackend)
rem echo 開啟內部後端專案...
rem cd C:\Project_official\COLDmain_backend
rem start npm run start

echo 開啟並執行檢點表專案
cd C:\Project_official\checkpointsys\checkpointsys
start npm run dev


echo 等待 2秒鐘後 後再執行...
timeout /t 2 /nobreak


rem 開啟並執行第三個專案 (MES_DashBoard)
echo 開啟MES製造執行系統專案...
cd C:\Project_official\MES_Main
start npm run start


endlocal


 