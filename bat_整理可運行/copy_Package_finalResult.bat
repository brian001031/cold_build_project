@echo off
setlocal

set "LOG=C:\BatteryAssembly\copy_Package_final.log"

echo ========================================== >> "%LOG%"
echo Start: %date% %time% >> "%LOG%"

set "SOURCE=\\192.168.3.101\BatteryAssembly"
set "DEST=\\192.168.3.100\HR_tmp\BatteryAssembly"
REM set "DEST=\\192.168.0.180\部門資料\BatteryAssembly"


echo SOURCE=%SOURCE% >> "%LOG%"
echo DEST=%DEST% >> "%LOG%"

echo Checking source... >> "%LOG%"

if not exist "%SOURCE%\" (
    echo [ERROR] Source not found >> "%LOG%"
    echo %date% %time% >> "%LOG%"
    endlocal
    exit /b 1
)

echo Source OK >> "%LOG%"

if not exist "%DEST%\" (
    echo Creating destination... >> "%LOG%"
    mkdir "%DEST%" >> "%LOG%" 2>&1
)

echo Start Robocopy... >> "%LOG%"

robocopy "%SOURCE%" "%DEST%" /E /Z /MT:32 /R:5 /W:5 >> "%LOG%" 2>&1

set "RC=%ERRORLEVEL%"

echo Robocopy ErrorLevel=%RC% >> "%LOG%"

if %RC% GEQ 8 (
    echo [ERROR] Copy failed >> "%LOG%"
) else (
    echo [OK] Copy completed >> "%LOG%"
)

echo End: %date% %time% >> "%LOG%"
echo ========================================== >> "%LOG%"

REM ==========================================
REM END BAT AND CLOSE CMD
REM ==========================================
endlocal
exit /b %RC%