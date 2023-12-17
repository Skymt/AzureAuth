$file = "${PSScriptRoot}\certificate.pfx"

#Exit if file exists
if (Test-Path $file) {
	Write-Host "The file certificate.pfx already exists." -ForegroundColor Red
	Exit 
}

#Check if administrator.
if ([Security.Principal.WindowsIdentity]::GetCurrent().Groups -contains 'S-1-5-32-544') {

	#Create / overwrite certificate.
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
	$press = Read-Host "Do you want start the import wizard now? (Y/N)"
	if ($press -eq "Y") { Start-Process -FilePath $file }
	else { Write-Host "Double-click the certificate file in the file explorer to start the wizard at a later time." }
}