@echo off
chcp 65001 >nul
setlocal EnableDelayedExpansion

:: 設定視窗標題
set "WINDOW_TITLE=KVALUE_WINDOW"

:: 啟動 Python 子視窗
::start "%WINDOW_TITLE%" cmd /K "python.exe C:\kvalue_convert.py"
start "" cmd /c "title %WINDOW_TITLE% & python.exe C:\kvalue_convert.py"

:: 等待子視窗啟動
::timeout /t 2 >nul
timeout /t 2 >nul

:: Debug: 顯示目前所有有視窗標題的程序
echo [DEBUG] 所有含視窗標題的 Process：
powershell -NoProfile -Command "Get-Process | Where-Object { $_.MainWindowTitle } | Select-Object Id,MainWindowTitle"

:: 嘗試取得 PID*(確保取得的PID為數字) /不再使用 -ExpandProperty，避免表格/加號符號等干擾
echo [DEBUG] 嘗試取得 PID for 視窗標題包含 "%WINDOW_TITLE%"...
for /f "usebackq delims=" %%i in (`powershell -NoProfile -Command "(Get-Process | Where-Object { $_.MainWindowTitle -like '*%WINDOW_TITLE%*' } | Select-Object -First 1 -ExpandProperty Id)"`) do (
    set "CMD_PID=%%i"
)

echo [DEBUG] CMD 視窗 PID = !CMD_PID!

if not defined CMD_PID (
    echo [ERROR] 無法取得 PID，請檢查視窗標題是否正確。
    pause
    exit /b 1
)

echo [INFO] 成功取得 PID = !CMD_PID!

::exit /b
::pause

:: === 等待 log 檔產出完成訊號 ===
:WAIT_FOR_DONE
timeout /t 1 >nul

powershell -NoProfile -Command ^
    "$pid = !CMD_PID!; ^
    if (Get-Process -Id $pid -ErrorAction SilentlyContinue) { ^
        $log = Get-Content -Path '%LOG_FILE%' -Raw -ErrorAction SilentlyContinue; ^
        if ($log -match '(\d{4}-\d{2}-\d{2}) (\d{2}:\d{2}:\d{2}) Data sync completed.') { ^
            $logTime = [datetime]::ParseExact($matches[1] + ' ' + $matches[2], 'yyyy-MM-dd HH:mm:ss', $null); ^
            $now = Get-Date; ^
            if ($logTime -ge $now.AddMinutes(-1) -and $logTime -le $now.AddMinutes(1)) { ^                
                exit 0 ^
            } else { 
				
				exit 1 			
			} ^
        } else { exit 1 } ^
    } else { exit 0 }"

if %errorlevel%==1 (
    goto WAIT_FOR_DONE
) 



rem 等待 5 秒鐘
::timeout /t 5 /nobreak

echo [INFO] 偵測到完成訊號，開始關閉所有含 "%WINDOW_TITLE%" 的視窗...
for /f "delims=" %%i in ('powershell -NoProfile -Command "Get-Process | Where-Object { $_.MainWindowTitle -like ''*%WINDOW_TITLE%*'' } | Select-Object -ExpandProperty Id"') do (
   echo [INFO] 嘗試殺掉 PID=%%i
   taskkill /PID %%i /F /T
)

echo [INFO] 任務完成。

exit /b

