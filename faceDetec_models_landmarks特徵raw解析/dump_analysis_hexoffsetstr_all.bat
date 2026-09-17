@echo off
setlocal EnableExtensions

cd /d "%~dp0"

echo ==========================================
echo TFLite ASCII / HEX Offset Analyzer
echo ==========================================
echo.

set "TFLITE=..\models\class_tflite\face_detector.tflite"
set "OUT=..\models\class_tflite\analysis_landinfo"

if not exist "%TFLITE%" (
echo [ERROR] 找不到：
echo %TFLITE%
echo.
pause
exit /b 1
)

if exist "%OUT%" rmdir /s /q "%OUT%"
mkdir "%OUT%"


echo [1/3] 建立 HEX dump...

rem filter hex type first
rem
rem 讀取 face_detector.tflite 的原始 Binary
rem 尋找其中連續可讀的 ASCII 字元
rem 記錄每一段 ASCII 字串的 HEX Offset
rem
rem ASCII 可讀範圍：
rem 0x20 = Space
rem 0x7E = ~
rem
rem 最短 ASCII 字串長度：
rem 4 characters
rem
rem Offset：
rem 使用 X8 格式輸出 8 位 HEX

certutil -dump "%TFLITE%" > "%OUT%\hex_dump.txt"

echo [2/3] 擷取 ASCII 字串...

powershell -NoProfile -ExecutionPolicy Bypass -Command "$b=[IO.File]::ReadAllBytes('%TFLITE%'); $s=New-Object Text.StringBuilder; $start=-1; for($i=0;$i -lt $b.Length;$i++){ if($b[$i] -ge 32 -and $b[$i] -le 126){ if($start -lt 0){$start=$i}; [void]$s.Append([char]$b[$i]) } else { if($start -ge 0 -and $s.Length -ge 4){ '{0} {1}' -f $start,$s.ToString() }; $s.Clear(); $start=-1 } }; if($start -ge 0 -and $s.Length -ge 4){ '{0} {1}' -f $start,$s.ToString() }" > "%OUT%\ascii_offset.txt"

echo  建立 ASCII 純文字列表...
rem ============================================================
rem 從 ascii_offset.txt 移除前面的 HEX Offset
rem 只保留 ASCII 字串
rem
rem 原始：
rem   00000124  TFLITE_METADATA
rem   00000158  MediaPipe
rem
rem 輸出：
rem   TFLITE_METADATA
rem   MediaPipe
rem ============================================================

powershell -NoProfile -ExecutionPolicy Bypass -Command "Get-Content '%OUT%\ascii_offset.txt' | ForEach-Object { if($_ -match '^\S+\s+(.*)$'){$matches[1]} }" > "%OUT%\ascii_strings.txt"

echo.
echo ==========================================
echo 分析完成
echo ==========================================
echo.

echo 輸出：
echo %OUT%\hex_dump.txt
echo %OUT%\ascii_offset.txt
echo %OUT%\ascii_strings.txt
echo.


echo ------------------------------------------
echo ASCII + HEX Offset:
echo ------------------------------------------
type "%OUT%\ascii_offset.txt"

echo.
echo ==========================================
pause
endlocal