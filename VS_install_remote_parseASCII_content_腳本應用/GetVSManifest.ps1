$manifestPath = Join-Path $PSScriptRoot "ChannelManifest.json"
$outputPath   = Join-Path $PSScriptRoot "VisualStudio.vsman"

if (-not (Test-Path $manifestPath)) {
    Write-Host "ERROR: ChannelManifest.json not found"
    Write-Host $manifestPath
    exit 1
}

# 不要用 Regex 解析 JSON ,使用 JSON parser
$text = Get-Content $manifestPath -Raw 

#try {
#    $manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json
#}
#catch {
#    Write-Host "ERROR: Invalid JSON"
#    Write-Host $_.Exception.Message
#    exit 1
#}

$pattern = '"fileName":"VisualStudio\.vsman".*?"sha256":"([^"]+)".*?"size":(\d+).*?"url":"([^"]+)"'


if ($text -notmatch $pattern) {
    Write-Host "ERROR: VisualStudio.vsman not found"
    exit 1
}


#$filterfile = $manifest.files | Where-Object {
#    $_.fileName -eq "VisualStudio.vsman"
#}


# 取得當前 URL、預期 SHA256 與預期檔案大小
$expectedHash = $matches[1]
$expectedSize = [int64]$matches[2]
$url          = $matches[3]




Write-Host "FOUND: VisualStudio.vsman"
Write-Host ""
Write-Host "Expected SHA256 : $expectedHash"
Write-Host "Expected SIZE   : $expectedSize"
Write-Host "URL             : $url"
Write-Host ""
Write-Host "Downloading..."


try {
    Invoke-WebRequest `
        -Uri $url `
        -OutFile $outputPath `
        -UseBasicParsing `
        -ErrorAction Stop
}
catch {
    Write-Host "ERROR: Download failed"
    Write-Host $_.Exception.Message
    exit 1
}

Write-Host "Download completed."
Write-Host ""


if (-not (Test-Path $outputPath)) {
    Write-Host "ERROR: Output file not found"
    exit 1
}

$actualSize = (Get-Item $outputPath).Length
$actualHash = (Get-FileHash $outputPath -Algorithm SHA256).Hash.ToLower()

Write-Host "Actual SIZE     : $actualSize"
Write-Host "Actual SHA256   : $actualHash"
Write-Host ""

# 實際URL遠程擷取驗證端點資料量size與當前文件不一致
if ($actualSize -ne $expectedSize) {
    Write-Host "FAIL: File size mismatch"
    exit 1
}

# Manifest size不等同 sha256 key   
if ($actualHash -ne $expectedHash.ToLower()) {
    Write-Host "FAIL: SHA256 mismatch"
    exit 1
}

# 下載下來的 VisualStudio.vsman 與你這份 ChannelManifest.json 宣告的檔案一致。
Write-Host "========================================"
Write-Host "PASS: VisualStudio.vsman verified"
Write-Host "========================================"