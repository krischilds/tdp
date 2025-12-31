$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$session.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0 Safari/537.36"
Invoke-WebRequest -UseBasicParsing -Uri "http://localhost:5201/users/me/features" `
-WebSession $session `
-Headers @{
"Accept"="application/json, text/plain, */*"
  "Accept-Encoding"="gzip, deflate, br, zstd"
  "Accept-Language"="en-US,en;q=0.9"
  "Authorization"="Bearer eyJhbGciOiJSUzI1NiIsImtpZCI6IjExMmJhYzE0OTk0MDQwOTM5YjJlMzU0ZmM4ZDMzNDYxIiwidHlwIjoiSldUIn0.eyJzdWIiOiIwZDY3ZWQzYS01OTk3LTRkN2MtOTAyMy04ZDAxNGYwMzBhNzkiLCJlbWFpbCI6ImtyaXMuY2hpbGRzQGdtYWlsLmNvbSIsIm5hbWUiOiJLcmlzIENoaWxkcyIsIm5iZiI6MTc2NzAyOTg5MCwiZXhwIjoxNzY3MDMwNzkwLCJpc3MiOiJodHRwOi8vbG9jYWxob3N0OjUyMDEiLCJhdWQiOiJ0ZHAtYXBpIn0.ifIrLouS_otMQCILVzypajG1-jQu9c_g23ToM5qJkaDzU8CX-UwpUoiha2WifgpelN33rLKljUy2tgnV2HwfxrIAbGXQtn9lO61YKP11iWQSFyqi5N_QgAasIO2r9Oo0NPMbRUTCAbo9Vs0P1wvDJX_prL6un-eOd2ZDtdYjEGRt1AlZyLwqevOmAY-SDZ9PdkaSKZ2AUThOcXJjGJPzQk-2PgWjryZwtuLPYBJ0dSwV7bC1DbF_DQH2zGBd8IRtDpgyY3K9Vuk8elN1YAAFwALsavrw1dDghFYeyMCZVobUh4l9H5cLVUqD5gif7JAHVm5Zxu27hRAqcVoGAOZucQ"
  "Origin"="http://localhost:5173"
  "Referer"="http://localhost:5173/"
  "Sec-Fetch-Dest"="empty"
  "Sec-Fetch-Mode"="cors"
  "Sec-Fetch-Site"="same-site"
  "sec-ch-ua"="`"Google Chrome`";v=`"143`", `"Chromium`";v=`"143`", `"Not A(Brand`";v=`"24`""
  "sec-ch-ua-mobile"="?0"
  "sec-ch-ua-platform"="`"Windows`""
}