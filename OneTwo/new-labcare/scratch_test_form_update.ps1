$headers = @{
    'Authorization' = 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJzbWFwbGVlbGFuZ29AZ21haWwuY29tIiwianRpIjoiNzRhM2ZkMTYtNDRkYS00NDkyLWFhMjAtMGJhODRhOWUzZTBmIiwidGVuYW50X2lkIjoiN2VjYjMxNjQtNGM3My00ZTViLThkZmItYzU5NzgzYmQ0ZDI0IiwidGVuYW50X2NvZGUiOiJURU41NzciLCJ0ZW5hbnRfbmFtZSI6ImVsYW5nbyIsInJvbGUiOiJUZW5hbnQiLCJleHAiOjE3ODIyMjYxMTEsImlzcyI6IkxJTVNfQVBJIiwiYXVkIjoiTElNU19Vc2VycyJ9._KR32QaZ7Rq8gisvsukbpH46WCugqJ8azmo5NzJKCjM'
    'tenant_code' = 'TEN577'
}

$boundary = [System.Guid]::NewGuid().ToString()
$LF = "`r`n"

$bodyFields = @{
    "user_code" = "57"
    "tenant_code" = "TEN577"
    "name" = "hbh"
    "short_name" = "jhb"
    "description" = "UPDATED_BY_FORM_DATA_PS"
    "mobile" = "jkn"
    "email" = "jd@gmail.com"
    "role" = "2"
    "module" = "General"
    "password" = '$2a$11$OYluiFmDyyULbvB94AjDlODZIDpJ6WrklajE/qAhDCg0054IsT60S'
}

$bodyBuilder = New-Object System.Text.StringBuilder

foreach ($key in $bodyFields.Keys) {
    [void]$bodyBuilder.Append("--$boundary$LF")
    [void]$bodyBuilder.Append("Content-Disposition: form-data; name=`"$key`"$LF$LF")
    [void]$bodyBuilder.Append("$($bodyFields[$key])$LF")
    
    # Also add user.key format
    [void]$bodyBuilder.Append("--$boundary$LF")
    [void]$bodyBuilder.Append("Content-Disposition: form-data; name=`"user.$key`"$LF$LF")
    [void]$bodyBuilder.Append("$($bodyFields[$key])$LF")
}

# Add branches
[void]$bodyBuilder.Append("--$boundary$LF")
[void]$bodyBuilder.Append("Content-Disposition: form-data; name=`"branchModel[0].bhcode`"$LF$LF")
[void]$bodyBuilder.Append("1$LF")

[void]$bodyBuilder.Append("--$boundary$LF")
[void]$bodyBuilder.Append("Content-Disposition: form-data; name=`"branchModel[0].user_code`"$LF$LF")
[void]$bodyBuilder.Append("57$LF")

[void]$bodyBuilder.Append("--$boundary$LF")
[void]$bodyBuilder.Append("Content-Disposition: form-data; name=`"branchModel[0].tenant_code`"$LF$LF")
[void]$bodyBuilder.Append("TEN577$LF")

[void]$bodyBuilder.Append("--$boundary$LF")
[void]$bodyBuilder.Append("Content-Disposition: form-data; name=`"branchModel[1].bhcode`"$LF$LF")
[void]$bodyBuilder.Append("56$LF")

[void]$bodyBuilder.Append("--$boundary$LF")
[void]$bodyBuilder.Append("Content-Disposition: form-data; name=`"branchModel[1].user_code`"$LF$LF")
[void]$bodyBuilder.Append("57$LF")

[void]$bodyBuilder.Append("--$boundary$LF")
[void]$bodyBuilder.Append("Content-Disposition: form-data; name=`"branchModel[1].tenant_code`"$LF$LF")
[void]$bodyBuilder.Append("TEN577$LF")

[void]$bodyBuilder.Append("--$boundary--$LF")

$body = $bodyBuilder.ToString()

Write-Host "--- Sending Form-Data update with password ---"
try {
    $res = Invoke-RestMethod -Uri 'https://labcareapi.iscansoft.com/api/User/update' -Headers $headers -Method Post -Body $body -ContentType "multipart/form-data; boundary=$boundary"
    Write-Host "Status: Success"
    $res | ConvertTo-Json -Depth 10
} catch {
    Write-Host "Status: Error"
    $errBody = [System.IO.StreamReader]($_.Exception.Response.GetResponseStream())
    $errText = $errBody.ReadToEnd()
    Write-Host "Error Body: $errText"
    Write-Error $_
}

$getUser = Invoke-RestMethod -Uri 'https://labcareapi.iscansoft.com/api/User/get?user_code=57' -Headers $headers -Method Get
Write-Host "After Form-Data update, Description is: $($getUser.description)"
