# update-sw-version.ps1
$swPath = Join-Path $PSScriptRoot "..\service-worker.published.js"
$versionPattern = "// Version updated at "
$versionStr = $versionPattern + "$(Get-Date -Format s)`r`n"

$swContent = Get-Content $swPath -Raw
$swContent = $swContent -replace "^$versionPattern.*`r`n", ''
$swContent = $versionStr + $swContent
Set-Content $swPath -Value $swContent -NoNewline
