Write-Host "========================================" -ForegroundColor Cyan
Write-Host "TEST SCRIPT START" -ForegroundColor Green
Write-Host "========================================"

Write-Host "[1] PSScriptRoot = [$PSScriptRoot]"

$BASE = $PSScriptRoot

Write-Host "[2] BASE = [$BASE]"

$TFLITE = Join-Path $BASE "class_tflite\face_detector.tflite"
$OUT    = Join-Path $BASE "class_tflite\analysis_landinfo"

$ASCII_OUT = Join-Path `
    -Path $OUT `
    -ChildPath "ascii_offset.txt"


$HEX_Formate_OUT = Join-Path `
    -Path $OUT `
    -ChildPath "hex_dump.txt"

	
$ASCII_STRINGS_OUT = Join-Path `
    -Path $OUT `
    -ChildPath "ascii_strings.txt"

#Write-Host "[3] TFLITE = [$TFLITE]"
#Write-Host "[3.1] OUT_PATH = [$OUT]"
#Write-Host "[3.2] ASCII_OUT = [$ASCII_OUT]"


$exists = Test-Path -LiteralPath $TFLITE -PathType Leaf

# -PathType Leaf 明確表示指定檔案
if (-not $exists){ 

  Write-Output "[ERROR] 找不到：" 
  Write-Output $TFLITE 
  Write-Output "" 
  Read-Host "按 Enter 繼續" 
  exit 1 
} 

Write-Host "[5] Test-Path result = [$exists]"

if (-not $exists) {

    Write-Host "[ERROR] File NOT FOUND!" -ForegroundColor Red
    Write-Host "Path = [$TFLITE]" -ForegroundColor Red

    exit 1
}

Write-Host "[6] File FOUND!" -ForegroundColor Green

$fileInfo = Get-Item -LiteralPath $TFLITE -ErrorAction Stop

Write-Host "[7] Get-Item completed!" -ForegroundColor Green

Write-Host "[DEBUG] FullName = [$($fileInfo.FullName)]"
Write-Host "[DEBUG] Name     = [$($fileInfo.Name)]"
#Write-Host "[DEBUG] Length   = [$($fileInfo.Length)] bytes"

if ($fileInfo.Length -eq 0) {

    Write-Host "[ERROR] File size is ZERO!" -ForegroundColor Red
    exit 1
}


if (Test-Path -LiteralPath $OUT) {
	#刪除
	Remove-Item -LiteralPath $OUT -Recurse -Force -ErrorAction Stop
}

New-Item -ItemType Directory -Path $OUT -Force -ErrorAction Stop | Out-Null


# Check output directory 是否有建立完成
if (-not (Test-Path -LiteralPath $OUT -PathType Container)) { 
   Write-Host "[ERROR] Output directory does not exist:" -ForegroundColor Red 
   Write-Host $OUT -ForegroundColor Red 
   exit 1 
}


Write-Host "[7] new create $OUT  profile OK !"
Write-Host "[8] ReadAllBytes starting..."

[byte[]]$data = [System.IO.File]::ReadAllBytes(
    $fileInfo.FullName
)

Write-Host "[9] ReadAllBytes completed!" -ForegroundColor Green
Write-Host "[DEBUG] data.Length = [$($data.Length)]"
Write-Host "[DEBUG] data.Type   = [$($data.GetType().FullName)]"

Write-Host "[1/3] prepare 分析 TFLite Binary..." -ForegroundColor Cyan


# ------------------------------------------------------------
 
# ASCII 暫存 , 最短 ASCII 長度 4

# ------------------------------------------------------------

$minLength = 4 
$strings = New-Object System.Collections.Generic.List[object]

# ------------------------------------------------------------

# ASCII 字串起始 Offset , 記錄每一段 ASCII 字串的 HEX Offset

# ------------------------------------------------------------

$start = -1


# ASCII 字串累積 Buffer
$asciiBuffer = ""

# ------------------------------------------------------------

# 掃描每一個 Byte

# ------------------------------------------------------------
# Printable ASCII：

for($i = 0; $i -lt $data.Length; $i++){
	
  # ASCII printable range: 0x20 ~ 0x7E
  if($data[$i] -ge 0x20 -and $data[$i] -le 0x7E){
	  
    $asciiChar = [char]$data[$i]


    #  尚未開始 ASCII 字串時，才記錄目前 Offset
	#  新 ASCII sequence 開始
    if($start -eq -1){
        $start = $i
		$asciiBuffer = ""
    }
	
	# 累積目前 ASCII 字元
    $asciiBuffer += $asciiChar
   
   
  }

  else{

    # ----------------------------------------------------
    # ASCII 字串結束 , End of ASCII sequence	
    # 遇到非 ASCII → 目前 sequence 結束
    # ----------------------------------------------------


    if($start -ge 0){

        $length = $i - $start

        if ($length -ge $minLength) {
						
		      $ascii = [System.Text.Encoding]::ASCII.GetString( $data,  $start, $length) 
			
			   
			  Write-Host "  Offset = $('{0:X8}' -f $start)"
			  Write-Host "  Length = $length"
			  Write-Host "  String = [$ascii]"

             
		      $strings.Add([PSCustomObject]@{
                  Offset = ('{0:X8}' -f $start) 
                  Length = $length 
                  String = $asciiBuffer 
              })
			  
			  Write-Host "[DEBUG] strings.Count = $($strings.Count)"

        }

        #$start.ToString('X8') + ' ' + $s.ToString()
    
	
		# 清除目前 ASCII Buffer
		$asciiBuffer = ""
			
		# 重設 Offset
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
           $ascii_endline = [System.Text.Encoding]::ASCII.GetString( $data,  $start, $length ) 

              $strings.Add([PSCustomObject]@{ 
               #$start.ToString('X8') + ' ' + $s.ToString()
                Offset = ('{0:X8}' -f $start) 
                Length = $length 
                String = $ascii_endline
              }) 

      } 
}


foreach ($item in $strings) {
    Write-Host "----------------------------------"
    Write-Host "Offset : [$($item.Offset)]"
    Write-Host "Length : [$($item.Length)]"
    Write-Host "String : [$($item.String)]"
}


# ============================================================ # Step 2 - 檢查 ASCII 掃描結果 # ============================================================
if ($strings.Count -eq 0) { 
    #Write-Host "[2/3] 找不到符合條件的 ASCII 字串。" -ForegroundColor Yellow	
	#Write-Host " 條件：Printable ASCII 0x20 ~ 0x7E，最短長度 $minLength。" -ForegroundColor Yellow 
	
	Write-Host "[2/3] No ASCII strings found." -ForegroundColor Yellow
	Write-Host "[INFO] Condition: Printable ASCII 0x20 ~ 0x7E, minimum length $minLength." -ForegroundColor Yellow
	
} 

 
if ($strings.Count -gt 0) {

    Write-Host "[2/3] Found $($strings.Count) ASCII strings." -ForegroundColor Cyan

}


# Check ASCII_OUT variable 
if ($null -eq $ASCII_OUT) { 

   Write-Host "[ERROR] ASCII_OUT is NULL." -ForegroundColor Red 
   exit 1 
   
}


if ([string]::IsNullOrWhiteSpace($ASCII_OUT)) {
    Write-Host "[ERROR] ASCII output path is empty." -ForegroundColor Red
    exit 1
}


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
Write-Host "Completed ConvertAscII track. Process 1 Run OK!" -ForegroundColor Green



# 產生原始 HEX Formate Byte 數列 

Write-Host "start 存取原始 HEX Binary Mapping ..." -ForegroundColor Cyan

Start-Sleep -Seconds 3

$hexLines = foreach ($offset in 0..(($data.Length - 1) / 16)) {

    # 每16位元作解取
    $start = $offset * 16

    $count = [Math]::Min(16, $data.Length - $start)

    $hex = ""

    for($j = 0; $j -lt $count; $j++){

        #取單字節 & 0x0F
        $hex += "{0:X2} " -f $data[$start + $j]
    }

    "{0:X8}  {1}" -f $start, $hex.TrimEnd()
}

$hexLines | Set-Content `
    -LiteralPath $HEX_Formate_OUT `
    -Encoding UTF8
	
	
Write-Host "[OK] HEX dump completed. Process 2 Run OK!" -ForegroundColor Green




Write-Host " 最後 final Extracting ASCII strings..." -ForegroundColor Cyan

Start-Sleep -Seconds 4

try {
	# 解取已經產生出ASCII表單
    Get-Content -LiteralPath $ASCII_OUT -ErrorAction Stop |
        ForEach-Object {

			# 將字元串接
            if ($_ -match '^\S+\s+(.*)$') {
                $matches[1]
            }

        } |
        Set-Content `
            -LiteralPath $ASCII_STRINGS_OUT `
            -Encoding UTF8 `
            -ErrorAction Stop

}
catch {

    Write-Host "[ERROR] Cannot create ascii_strings.txt" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}


Write-Host "[OK] ASCII STRINGS contact Full. Process 3 Run OK!" -ForegroundColor Green
Write-Host "TOTAL SCRIPT COMPLETE !" -ForegroundColor Green
