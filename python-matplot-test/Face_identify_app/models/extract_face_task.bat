@echo off
setlocal

cd /d "%~dp0"

echo ==========================================
echo  MediaPipe TASK extractor
echo ==========================================
echo.

set "TASK=..\models\face_landmarker.task"
set "OUT=..\models\class_tflite"

echo TASK = %TASK%
echo OUT  = %OUT%
echo.

if not exist "%TASK%" (
echo [ERROR] 找不到 TASK：
echo %TASK%
echo.
pause
exit /b 1
)

if exist "%OUT%" (
echo [INFO] 移除舊的 model 資料夾...
rmdir /s /q "%OUT%"
)

mkdir "%OUT%"

echo [INFO] 解包 %TASK%
echo.

tar -xf "%TASK%" -C "%OUT%"

if errorlevel 1 (
echo.
echo [ERROR] TASK 解包失敗。
echo.
pause
exit /b 2
)

echo.
echo ==========================================
echo  解包完成
echo ==========================================
echo.

dir /s /b "%OUT%"

echo.
echo [NEXT]
echo 下一階段再針對 .tflite 做 ASCII / metadata / HEX offset 分析。
echo.

pause
endlocal
