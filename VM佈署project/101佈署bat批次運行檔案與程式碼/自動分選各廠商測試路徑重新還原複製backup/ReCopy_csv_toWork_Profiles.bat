@echo off
rem 開始執行

rem 等待 1 秒鐘
timeout /t 1 /nobreak

rem 在同一命令視窗中執行 Python 腳本 copy_currentyear_pfcccsv.py 並保留命令視窗
start cmd /K "python.exe C:\copy_currentyear_pfcccsv.py"

if errorlevel 1 (
    echo copy_currentyear_pfcccsv.py 執行錯誤，結束執行
    exit /b 1
)

rem 完成
endlocal
