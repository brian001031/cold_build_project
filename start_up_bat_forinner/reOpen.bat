@echo off
title COLDELECTRIC 系統啟動器
setlocal

:start
cls
echo ==========================================
echo  COLDELECTRIC 回報系統啟動中...
echo ==========================================

rem 1. 先強制關閉舊的進程 (選用，避免 Port 衝突)
echo [1/5] 正在清理舊的程序...
taskkill /f /im node.exe 2>nul
taskkill /f /im gitea.exe 2>nul
taskkill /f /im redis-server.exe 2>nul
taskkill /f /im python.exe 2>nul

rem 2. 啟動網站後端
echo [2/5] 正在啟動網站後端...
if exist "C:\Users\Administrator\Desktop\officialProject\COLDmain_backend" (
    cd /d "C:\Users\Administrator\Desktop\officialProject\COLDmain_backend"
    start "COLD_Backend" node auto_restart.js
) else (
    echo [錯誤] 找不到後端路徑！
)

rem 3. 啟動 Gitea
echo [3/5] 正在啟動 Gitea 專案...
if exist "C:\Users\Administrator\Desktop\gitea" (
    cd /d "C:\Users\Administrator\Desktop\gitea"
    start "Gitea_Server" gitea.exe
) else (
    echo [錯誤] 找不到 Gitea 路徑！
)

rem 4. 啟動 redis 
echo [4/5] 正在啟動 redis...
if exist "C:\Users\Administrator\Desktop\officialProject\redis_Local" (
    cd /d "C:\Users\Administrator\Desktop\officialProject\redis_Local"
    start "Redis_Server" redis-server.exe redis.windows.conf
) else (
    echo [錯誤] 找不到 Redis 路徑！
)

rem 5. 啟動 python backend
echo [5/5] 正在啟動 python 專案...
if exist "C:\Users\Administrator\Desktop\officialProject\python_backend" (
    cd /d "C:\Users\Administrator\Desktop\officialProject\python_backend"
    start "Python_Backend" cmd /k ".\venv\Scripts\activate && uvicorn app.main:create_app --factory --host 0.0.0.0 --port 8000 --reload"
) else (
    echo [錯誤] 沒有正確啟動 python 專案（路徑不存在）
)

timeout /t 2 /nobreak >nul

echo.
echo ==========================================
echo  所有服務已嘗試啟動完成。
echo ==========================================
pause