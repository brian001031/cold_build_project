@echo off
rem 開始執行

rem 等待 2 秒鐘
timeout /t 2 /nobreak

rem 在同一命令視窗中執行 Python 腳本 kvalue_convert.py 並保留命令視窗
start cmd /K "python.exe C:\kvalue_convert.py"

if errorlevel 1 (
    echo kvalue_convert.py 執行錯誤，結束執行
    exit /b 1
)

rem 完成
endlocal