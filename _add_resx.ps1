$viPath = 'd:\BlazorWasmPortfolioGhAction\Resources\SharedResources.resx'
$enPath = 'd:\BlazorWasmPortfolioGhAction\Resources\SharedResources.en.resx'

$viEntries = @{
    'Common_Loading' = 'Đang tải...'
    'Common_Searching' = 'Đang tìm...'
    'Common_Saving' = 'Đang lưu...'
    'Common_LoadingData' = 'Đang tải dữ liệu'
    'Common_LoadingAria' = 'Đang tải nội dung'
    'Nav_LoadingPage' = 'Đang tải trang'
}

$enEntries = @{
    'Common_Loading' = 'Loading...'
    'Common_Searching' = 'Searching...'
    'Common_Saving' = 'Saving...'
    'Common_LoadingData' = 'Loading data'
    'Common_LoadingAria' = 'Loading content'
    'Nav_LoadingPage' = 'Loading page'
}

function Add-Entries($path, $entries, $lastKey) {
    $xml = [System.IO.File]::ReadAllText($path, [System.Text.Encoding]::UTF8)
    $anchor = '</root>'
    $sb = New-Object System.Text.StringBuilder
    foreach ($k in $entries.Keys) {
        $v = $entries[$k]
        $sb.Append("<data name=`"$k`" xml:space=`"preserve`"><value>$v</value></data>") | Out-Null
    }
    $insert = $sb.ToString()
    $new = $xml.Replace($anchor, $insert + $anchor)
    [System.IO.File]::WriteAllText($path, $new, (New-Object System.Text.UTF8Encoding($false)))
    Write-Host "Added to $path"
}

Add-Entries $viPath $viEntries
Add-Entries $enPath $enEntries