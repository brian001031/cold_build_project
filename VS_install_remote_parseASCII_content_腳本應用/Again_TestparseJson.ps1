$manifestPath = Join-Path $PSScriptRoot "ChannelManifest.json"
$outputPath   = Join-Path $PSScriptRoot "VisualStudio.vsman"

if (-not (Test-Path $manifestPath)) {
    Write-Host "ERROR: ChannelManifest.json not found"
    Write-Host $manifestPath
    exit 1
}

try {
    $jsonText = Get-Content $manifestPath -Raw

    if ([string]::IsNullOrWhiteSpace($jsonText)) {
        Write-Host "ERROR: ChannelManifest.json is empty"
        exit 1
    }

    $manifest = $jsonText | ConvertFrom-Json -ErrorAction Stop
}
catch {
    Write-Host "ERROR: Invalid JSON"
    Write-Host $_.Exception.Message
    exit 1
}

Write-Host "JSON parsed successfully."
Write-Host "Manifest properties:"

$manifest.PSObject.Properties.Name | ForEach-Object {
    Write-Host " - $_"
}
