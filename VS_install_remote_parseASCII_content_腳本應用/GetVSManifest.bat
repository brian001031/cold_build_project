@echo off
setlocal


echo BAT: %~f0
echo PS1: %~dp0GetVSManifest.ps1
echo.


rem 中文全部拿掉
"%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -File "%~dp0GetVSManifest.ps1"

echo.
echo ExitCode=%ERRORLEVEL%
pause