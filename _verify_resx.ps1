$vi = [System.IO.File]::ReadAllText('d:\BlazorWasmPortfolioGhAction\Resources\SharedResources.resx', [System.Text.Encoding]::UTF8)
$en = [System.IO.File]::ReadAllText('d:\BlazorWasmPortfolioGhAction\Resources\SharedResources.en.resx', [System.Text.Encoding]::UTF8)
foreach ($k in @('Common_Loading','Common_Searching','Common_Saving','Common_LoadingData','Common_LoadingAria','Nav_LoadingPage')) {
    $viI = $vi.IndexOf($k + '"')
    $enI = $en.IndexOf($k + '"')
    Write-Host "=== $k ==="
    if ($viI -ge 0) { Write-Host "VI: $($vi.Substring($viI, 80))" } else { Write-Host "VI: NOT FOUND" }
    if ($enI -ge 0) { Write-Host "EN: $($en.Substring($enI, 80))" } else { Write-Host "EN: NOT FOUND" }
}