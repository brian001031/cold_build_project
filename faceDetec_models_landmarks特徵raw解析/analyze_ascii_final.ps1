$BASE = $PSScriptRoot

$TFLITE = Join-Path $BASE "..\models\class_tflite\face_detector.tflite"
$OUT    = Join-Path $BASE "..\models\class_tflite\analysis_landinfo"


Write-Host "TFLite ASCII / HEX Offset Analyzer" -ForegroundColor Cyan

# -PathType Leaf 明確表示指定檔案
if (-not (Test-Path -LiteralPath $TFLITE -PathType Leaf)){ 

  Write-Output "[ERROR] 找不到：" 
  Write-Output $TFLITE 
  Write-Output "" 
  Read-Host "按 Enter 繼續" 
  exit 1 
} 


try {
    if (Test-Path -LiteralPath $OUT) {
        Remove-Item -LiteralPath $OUT -Recurse -Force -ErrorAction Stop
    }

    New-Item -ItemType Directory -Path $OUT -Force -ErrorAction Stop | Out-Null
}
catch {
    Write-Host "[ERROR] 無法建立輸出目錄：" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}



$ASCII_OUT = Join-Path -Path $OUT -ChildPath "ascii_offset.txt"






Write-Host "[1/3] 分析 TFLite Binary..." -ForegroundColor Cyan



# ============================================================

# TFLite ASCII + HEX Offset Analyzer
#

# $args[0] = TFLite 路徑

# $args[1] = ASCII Offset 輸出檔

# ============================================================

#$TFLITE = $args[0]

#$ASCII_OUT = $args[1]

# ------------------------------------------------------------

# 讀取 TFLite 原始 Binary

# ------------------------------------------------------------

# Read TFLITE as raw binary  ,尋找連續可讀的 ASCII 字元
try {
    [byte[]]$data = [System.IO.File]::ReadAllBytes($TFLITE)
}
catch {
    Write-Host "[ERROR] 無法讀取 TFLite：" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

# ------------------------------------------------------------
 
# ASCII 暫存 , 最短 ASCII 長度 4

# ------------------------------------------------------------

$minLength = 4 
$strings = New-Object System.Collections.Generic.List[object]

# ------------------------------------------------------------

# ASCII 字串起始 Offset , 記錄每一段 ASCII 字串的 HEX Offset

# ------------------------------------------------------------

$start = -1

# ------------------------------------------------------------

# 掃描每一個 Byte

# ------------------------------------------------------------
# Printable ASCII：


for($i = 0; $i -lt $data.Length; $i++){

  # ASCII printable range: 0x20 ~ 0x7E
  if($data[$i] -ge 0x20 -and $data[$i] -le 0x7E){

    #尚未開始 ASCII 字串時，才記錄目前 Offset
    if($start -eq -1){
        $start = $i
    }
   
  }

  else{

    # ASCII 字串結束 , End of ASCII sequence

    if($start -ge 0){

        $length = $i - $start

        if ($length -ge $minLength) {

		$ascii = [System.Text.Encoding]::ASCII.GetString( 
                     $data, 
                     $start, 
                     $length 
                   ) 


              $strings.Add([PSCustomObject]@{ 
                  Offset = ('{0:X8}' -f $start) 
                  Length = $length 
                  String = $ascii 
              })

        }

     #   $start.ToString('X8') + ' ' + $s.ToString()
    
	
		
      $start = -1

    }
  }

}



# ------------------------------------------------------------

# 處理檔案最後一段 ASCII

# ------------------------------------------------------------

if ($start -ge 0) { 

       $length = $data.Length - $start 

       if ($length -ge $minLength) { 
           $ascii = [System.Text.Encoding]::ASCII.GetString( 
                $data, 
                $start, 
                $length 
               ) 

              $strings.Add([PSCustomObject]@{ 

               #$start.ToString('X8') + ' ' + $s.ToString()
                Offset = ('{0:X8}' -f $start) 
                Length = $length 
                String = $ascii 
              }) 

      } 
}


# ============================================================ # Step 2 - 檢查 ASCII 掃描結果 # ============================================================
if ($strings.Count -eq 0) { 
    #Write-Host "[2/3] 找不到符合條件的 ASCII 字串。" -ForegroundColor Yellow	
	#Write-Host " 條件：Printable ASCII 0x20 ~ 0x7E，最短長度 $minLength。" -ForegroundColor Yellow 
	
	Write-Host "[2/3] No ASCII strings found." -ForegroundColor Yellow
	Write-Host "[INFO] Condition: Printable ASCII 0x20 ~ 0x7E, minimum length $minLength." -ForegroundColor Yellow
	
} else { 
	   #Write-Host "[2/3] 找到 $($strings.Count) 組 ASCII 字串..." -ForegroundColor Cyan 
	   Write-Host "[2/3] Found $($strings.Count) ASCII strings." -ForegroundColor Cyan
}



# 將輸出寫入檔案

# ------------------------------------------------------------

#} | Set-Content -LiteralPath $ASCII_OUT -Encoding UTF8

#{ 
#
#   Write-Output "00000120 TensorFlow" 
#   Write-Output "000003A4 version" 
#   Write-Output "00001F20 face_detector" 
#} | Set-Content -LiteralPath $ASCII_OUT -Encoding UTF8


# Check ASCII_OUT variable 
if ($null -eq $ASCII_OUT) { 

   Write-Host "[ERROR] ASCII_OUT is NULL." -ForegroundColor Red 
   exit 1 
   
}



if ([string]::IsNullOrWhiteSpace($ASCII_OUT)) {
    Write-Host "[ERROR] ASCII output path is empty." -ForegroundColor Red
    exit 1
}

# Check output directory 
if (-not (Test-Path -LiteralPath $OUT -PathType Container)) { 
   Write-Host "[ERROR] Output directory does not exist:" -ForegroundColor Red 
   Write-Host $OUT -ForegroundColor Red 
   exit 1 
}

Write-Host "[DEBUG] ASCII output: $ASCII_OUT"
Write-Host "[DEBUG] BASE      = [$BASE]"
Write-Host "[DEBUG] TFLITE    = [$TFLITE]"
Write-Host "[DEBUG] OUT       = [$OUT]"
Write-Host "[DEBUG] ASCII_OUT = [$ASCII_OUT]"


try { 
  
  $outputLines = @(
     $strings | ForEach-Object { 
	    "{0} {1} {2}" -f $_.Offset, $_.Length, $_.String 
	 } 
  ) 
  
	 Set-Content `
	    -LiteralPath $ASCII_OUT `
		-Value $outputLines `
		-Encoding UTF8 `
		-ErrorAction Stop 
		
} catch {
	
	Write-Host "[ERROR] can not write in File!" -ForegroundColor Red 
	Write-Host $_.Exception.Message -ForegroundColor Red 
	exit 1 
} 

Write-Host "[3/3] Output completed:" -ForegroundColor Cyan
Write-Host $ASCII_OUT 
Write-Host "" 
Write-Host "Completed. finish" -ForegroundColor Green