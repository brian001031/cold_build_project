@echo off
chcp 65001 > nul
setlocal enabledelayedexpansion


set /p "searchpath=請輸入要分析的資料夾（例如 D:\Data）："
set /p maxSize=請輸入要列出檔案的最大大小（bytes）： 

:: ✅ 檢查目錄是否存在
if not exist "!searchpath!" (
    echo 錯誤：指定的資料夾不存在。
    pause
    exit /b
)

::先刪除既有log檔案
del reachsize_list.txt 2>nul
del sorted_size_list.txt 2>nul

echo Debug: searchpath = "!searchpath!"
echo Debug: maxSize = "!maxSize!"

if not exist "!searchpath!" (
    echo 錯誤：指定的資料夾不存在。
    pause
    exit /b
)


pushd "!searchpath!"
for /r %%i in (*) do (
    set "size=%%~zi"
    echo ScanFile: %%~nxi size=!size!
    if !size! LSS !maxSize! (
        for /f "tokens=1-4*" %%a in ('dir /T:W /-C /A:-D "%%i" ^|findstr /R "^[0-9]"') do (
            echo %%~nxi^|%%~zi bytes ^| Last Modified: %%a %%b %%c >>  "%~dp0reachsize_list.txt"
        )
    )
)
popd


echo Sorting file: "%~dp0reachsize_list.txt"
if exist "%~dp0reachsize_list.txt" (
    echo File exists, content below:
    type "%~dp0reachsize_list.txt"
) else (
    echo ERROR: reachsize_list.txt not found
    pause & exit /b
)

rem 接著排序
sort /R "%~dp0reachsize_list.txt" /o "%~dp0sorted_size_list.txt"
::sort "%~dp0reachsize_list.txt" /R "%~dp0sorted_size_list.txt"


echo After sorting, sorted file content:
type "%~dp0sorted_size_list.txt"


echo.
echo ✅ 完成！已產生自訂義找尋 %~dp0reachsize_list.txt
echo 最新修改日期排序 %~dp0sorted_size_list.txt
pause
