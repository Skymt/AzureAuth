$file = "${PSScriptRoot}\certificate.pfx"

#Exit if file exists
if (Test-Path $file) {
	Write-Host "The file certificate.pfx already exists." -ForegroundColor Red
	Exit 
}

#Check if administrator.
if ([Security.Principal.WindowsIdentity]::GetCurrent().Groups -contains 'S-1-5-32-544') {

	#Create certificate file.
	$cert = New-SelfSignedCertificate -Subject localhost -DnsName localhost -FriendlyName "Functions Development" -KeyUsage DigitalSignature -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.1")
	Export-PfxCertificate -Cert $cert -FilePath $file -Password (ConvertTo-SecureString -String "123456" -Force -AsPlainText)

} else {
	#Re-run this script as administrator.
    Write-Host "Enter administrator mode to generate certificate file. (Y/N)" -ForegroundColor Yellow -NoNewline
	$press = Read-Host
	if (-not($press -eq "Y")) { Exit }
	Start-Process -FilePath PowerShell -ArgumentList $PSCommandPath -Verb RunAs

	#Wait for the file to be created.
	While (-not (Test-Path $file)) { Start-Sleep -Seconds 1 }

	#Inform the user of the generated file and its password.
	Write-Host "The certificate file has been generated with password '123456'." -ForegroundColor Green
	Write-Host "It should probably be imported into the Trusted Root Certification Authorities store." -ForegroundColor Green
	Write-Host "You can do this in the windows Import Certificate wizard." -ForegroundColor Green
	Write-Host
	#Start the import wizard upon user request.
	Write-Host "Do you want start the import wizard now? (Y/N)" -ForegroundColor Yellow -NoNewline
	$press = Read-Host
	if ($press -eq "Y") { Start-Process -FilePath $file }
	else { Write-Host "Double-click the certificate file in the file explorer to start the wizard at a later time." -ForegroundColor Green }
}
# SIG # Begin signature block
# MIIFhQYJKoZIhvcNAQcCoIIFdjCCBXICAQExCzAJBgUrDgMCGgUAMGkGCisGAQQB
# gjcCAQSgWzBZMDQGCisGAQQBgjcCAR4wJgIDAQAABBAfzDtgWUsITrck0sYpfvNR
# AgEAAgEAAgEAAgEAAgEAMCEwCQYFKw4DAhoFAAQUCkO+TRPO7FgzAc0aRCJvSULu
# BRmgggMYMIIDFDCCAfygAwIBAgIQWb9/UJsf9opBppV5vzGcETANBgkqhkiG9w0B
# AQsFADAiMSAwHgYDVQQDDBdQb3dlclNoZWxsIENvZGUgU2lnbmluZzAeFw0yMzEy
# MTkyMjQyNTdaFw0yNDEyMTkyMzAyNTdaMCIxIDAeBgNVBAMMF1Bvd2VyU2hlbGwg
# Q29kZSBTaWduaW5nMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA5+SZ
# eyj54VjhTKaWzS++MC9jc7rv/7JoVgUkb0fMsFpatKMAJBwAM7HErCiP4u4uc4w/
# I/b5HKPzBWbcTwhTnezPMJ7h1iBBUoGHDS2Mjm1HlFnlrJvguepdOm8Sk5S4E+6a
# Z8oUI17C5ttpongFA9M4zYPtFX3HzWacddRgmENTOLCHeb/VNlYnP/eX3405+fZb
# mIIc7FgzGWPdW1zAeeuKLyZs81NGGX/bHkUv1FF8yoElA5MHJHdFP3SGh1zuPsyz
# Wr7p2hsItXN27//HIrkN+LVNlnvx4yrFUjendlj+guHKJpVBr8Kk5RqQSllc1Pif
# WLD0shz4MFYMckxN2QIDAQABo0YwRDAOBgNVHQ8BAf8EBAMCB4AwEwYDVR0lBAww
# CgYIKwYBBQUHAwMwHQYDVR0OBBYEFLB781ewCocKVOdU4FtSXMdE5WTMMA0GCSqG
# SIb3DQEBCwUAA4IBAQC0w1XWGpkon0QrlLj8LhyFPMTOzo+IeAz2Mah4yNEAlEo0
# W/nIsObXObIpOE78nAdcQRZn+8kPoghaBYerYNYpgVAsUF+MRzXa518MSanvOGVm
# f99R947e9FVa7+OOE/+OkI+sVTyw5semDDDw3n/xN9fvRS8sW4rEygsy2CRGVo7X
# jBl6gC1tWIoniNm0NQjt0Z901hCKyGl2kzXm2LfblEhgwPLGf2ju/wfdJGdhaDIq
# 2qyiE/ACeXKgK96ZBapE6tlEUEESXvQu5a4il+IIeqc5MXAGjuU1B8Y0rROxNJFw
# +AB0OHhpbLV3I3YDVD3VO1qWUW8eBW58U+Mln9klMYIB1zCCAdMCAQEwNjAiMSAw
# HgYDVQQDDBdQb3dlclNoZWxsIENvZGUgU2lnbmluZwIQWb9/UJsf9opBppV5vzGc
# ETAJBgUrDgMCGgUAoHgwGAYKKwYBBAGCNwIBDDEKMAigAoAAoQKAADAZBgkqhkiG
# 9w0BCQMxDAYKKwYBBAGCNwIBBDAcBgorBgEEAYI3AgELMQ4wDAYKKwYBBAGCNwIB
# FTAjBgkqhkiG9w0BCQQxFgQUIWKq0AXZmICBdxgESa7P0nGXpaswDQYJKoZIhvcN
# AQEBBQAEggEAkS6HdZ07lL/IKS1XkyKnjGFZ3sP3qncM7JNMBoTQ0gd2f7DcQsop
# ayg85ymVIHFi/fKx4LRdX5IDB/VeDDDDFHTl7LVcCVEagYEr6jX3Lt2sf8vohIzW
# vGfDHcKqXT15xaMgUYq7Ue/dnm3bkqEEfHDdd1xpI2qZA91/alEwLCm/QxtzwLIo
# 4vn+oz91NwBDIJcOqXB/rjLyjxjkhYegCIm5WM6HnSBmgaCORBde2pWoFkxwYFh9
# QMM2m5QAXEtSr+LF7wGC+QEQUp1w0v3Aw06enFcS6m/GriwRvT8z6/HatudbMRG1
# v15CbFG+xllHXqbPXl5v47oJrupxCJdiTQ==
# SIG # End signature block
