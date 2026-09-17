$path = "C:\vs_installer\VS2019_Professional_DEFINETOOL\VisualStudio.vsman"

Write-Host "=== 1. FILE TEST ===" -ForegroundColor Cyan
Write-Host "Exists:" (Test-Path $path)

if (Test-Path $path) {
    $f = Get-Item $path
    Write-Host "Size:" $f.Length "bytes"
    Write-Host "LastWriteTime:" $f.LastWriteTime
}

Write-Host ""
Write-Host "=== 2. FIRST 100 CHARACTERS ===" -ForegroundColor Cyan

$stream = [System.IO.File]::OpenRead($path)
$reader = New-Object System.IO.StreamReader($stream, [System.Text.Encoding]::UTF8)
$text = $reader.ReadLine()
$reader.Close()
$stream.Close()

Write-Host $text.Substring(0, [Math]::Min(100, $text.Length))

Write-Host ""
Write-Host "=== 3. JSON TEST ===" -ForegroundColor Cyan

try {
    $jsonText = [System.IO.File]::ReadAllText($path)
    Write-Host "ReadAllText OK" -ForegroundColor Green
    Write-Host "Characters:" $jsonText.Length

    $json = $jsonText | ConvertFrom-Json

    Write-Host "ConvertFrom-Json OK" -ForegroundColor Green
    Write-Host "manifestVersion:" $json.manifestVersion
}
catch {
    Write-Host "ERROR:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

Write-Host ""
Write-Host "=== DONE ===" -ForegroundColor Cyan
