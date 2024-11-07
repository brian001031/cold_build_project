@echo off
setlocal

REM 設定來源和目標資料夾
::set SOURCE=\\192.168.3.100\hr_tmp\source_pfcc
set SOURCE=\\192.168.3.101\pf-cc
set DEST=\\192.168.3.100\HR_tmp\pfcc_result



set "username=Administrator"
set "password=P@ssw0rd"


::xcopy "\\192.168.3.100\hr_tmp\source_pfcc\*" "\\192.168.3.101\C:\source_pfcc\" /E /I /Y 
robocopy %SOURCE% %DEST%  /S /DCOPY:DA /COPY:DAT /Z /MT:100 /R:5 /W:5 


REM 檢查退出碼
if %ERRORLEVEL% leq 7 (
    echo 複製成功
) else (
    echo 複製失敗，錯誤碼: %ERRORLEVEL%
)

endlocal
pause