@echo off
setlocal

rem 中文全部拿掉
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Again_TestparseJson.ps1"


echo.
pause