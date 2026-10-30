@echo off
rem 開始執行

:loop
echo coldelectric回報系統正在重啟執行.....

rem 開啟並執行第一個專案 (innerWeb)
echo 開啟內部報修專案...
cd C:\Users\Administrator\Desktop\Bert-Project\innerWeb
start npm run start

echo 等待 2秒鐘後 後再執行...
timeout /t 2 /nobreak

rem 開啟並執行第二個專案 (innerbackend)
echo 開啟內部後端專案...
cd C:\Users\Administrator\Desktop\Bert-Project\innerbackend
start npm run start

rem 自動啟動XAMPP特定服務（ Apache 和 MySQL）
cd /d C:\XAMPP
start xampp-control.exe

start apache service
net start Apache2.4.58

echo 等待 1秒鐘後 後再執行...
timeout /t 1 /nobreak

start mysql service
net start MySQL

xampp_start.exe apache

echo 等待 1秒鐘後 後再執行...
timeout /t 1 /nobreak

xampp_start.exe mysql

endlocal


 