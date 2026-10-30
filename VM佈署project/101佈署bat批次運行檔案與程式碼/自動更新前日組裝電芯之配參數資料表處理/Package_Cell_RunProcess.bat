@echo off
rem 開始執行

rem 等待 3 秒鐘
timeout /t 3 /nobreak

rem 在同一命令視窗中執行 Python 腳本 package_final_combine_action_release.py ,bat dependon py sesssion finsh complete close cmd
start "" /wait cmd /C "python.exe C:\package_final_combine_action_release.py"

if errorlevel 1 (
	echo.
    echo [ERROR] package_final_combine_action_release.py 執行錯誤
    exit /b 1
)


echo.
echo [OK] Python process completed.
exit /b 0


