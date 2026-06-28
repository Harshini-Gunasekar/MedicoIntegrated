$tenantCode = "TEN1011"
$headers = @{
    "tenant_code" = $tenantCode
    "Content-Type" = "application/json"
}

Write-Output "--- Test 1: POST http://medicoapi.iscansoft.com/api/Customer/search with name = Ankit ---"
$body1 = @{ name = "Ankit"; tenant_code = $tenantCode } | ConvertTo-Json
try {
    $res1 = Invoke-RestMethod -Uri "http://medicoapi.iscansoft.com/api/Customer/search" -Method Post -Headers $headers -Body $body1
    Write-Output "Status: Success, Count: $($res1.Count)"
    if ($res1.Count -gt 0) {
        $res1[0] | Format-List custid, name, mobile, custcode
    }
} catch {
    Write-Error $_
}

Write-Output "`n--- Test 2: GET http://medicoapi.iscansoft.com/api/Customer/search?key=Ankit ---"
try {
    $res2 = Invoke-RestMethod -Uri "http://medicoapi.iscansoft.com/api/Customer/search?key=Ankit" -Method Get -Headers $headers
    Write-Output "Status: Success, Count: $($res2.Count)"
    if ($res2.Count -gt 0) {
        $res2[0] | Format-List custid, name, mobile, custcode
    }
} catch {
    Write-Error $_
}
