@echo off
setlocal

REM 設定來源和目標資料夾
set SOURCE=\\192.168.3.101\copy_temp\pf-cc-testNG
set DEST=\\192.168.3.100\HR_tmp\pf-cc-result_NG
set "FILENAME=error_record.txt"



set "username=Administrator"
set "password=P@ssw0rd"


::xcopy "\\192.168.3.100\hr_tmp\source_pfcc\*" "\\192.168.3.101\C:\source_pfcc\" /E /I /Y 
::robocopy %SOURCE% %DEST%  /S /DCOPY:DA /COPY:DAT /Z /MT:100 /R:5 /W:5 

REM 用 robocopy 複製指定檔案
robocopy "%SOURCE%" "%DEST%" "%FILENAME%" /DCOPY:DA /COPY:DAT /Z /MT:100 /R:5 /W:5


REM 用 xcopy 複製單一檔案
::xcopy "%SOURCE%" "%DEST%" /Y /I

REM 檢查退出碼
if %ERRORLEVEL% leq 7 (
    echo 複製error_record.txt成功
) else (
    echo 複製error_record.txt失敗，錯誤碼: %ERRORLEVEL%
)

endlocal
pause