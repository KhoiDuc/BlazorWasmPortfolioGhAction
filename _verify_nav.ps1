$en = [System.IO.File]::ReadAllText('d:\BlazorWasmPortfolioGhAction\Resources\SharedResources.en.resx', [System.Text.Encoding]::UTF8)
$i = $en.IndexOf('Nav_LoadingPage')
Write-Host "Index: $i"
Write-Host "Length: $($en.Length)"
Write-Host "Tail: $($en.Substring($i))"