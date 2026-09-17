@echo off
setlocal

rem 中文全部拿掉
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0GetVSManifest.ps1"


echo.
pause