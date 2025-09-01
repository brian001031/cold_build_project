@echo off
rem 開始執行

:loop
echo coldelectric回報系統正在重啟執行.....

rem 開啟並執行第一個專案 (innerWeb)
echo 開啟內部報修及銷庫存網頁專案...
cd C:\Project_official\COLDmain_frontweb
start npm run start

echo 等待 2秒鐘後 後再執行...
timeout /t 2 /nobreak

rem 開啟並執行第二個專案 (innerbackend)
echo 開啟內部後端專案...
cd C:\Project_official\COLDmain_backend
start npm run start


echo 等待 2秒鐘後 後再執行...
timeout /t 2 /nobreak


rem 開啟並執行第三個專案 (MES_DashBoard)
echo 開啟MES製造執行系統專案...
cd C:\Project_official\MES_Main
start npm run start


endlocal


 