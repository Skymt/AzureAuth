$file = "${PSScriptRoot}\certificate.pfx"

#Exit if file exists
if (Test-Path $file) {
	Write-Host "The file certificate.pfx already exists." -ForegroundColor Red
	Exit 
}

#Check if administrator.
if ([Security.Principal.WindowsIdentity]::GetCurrent().Groups -contains 'S-1-5-32-544') {
	Write-Host "This script will generate a self-signed certificate for local development." -ForegroundColor Green
	Write-Host "The certificate can then be found in the certmgr (Certificate Manager). Would you like to start certmgr now? (Y/N)" -ForegroundColor Green
	$answer = Read-Host
	if ($answer -eq "Y") { 
		Start-Process certmgr
	}

	$cert = New-SelfSignedCertificate -Subject localhost -DnsName localhost -FriendlyName "Functions Development" -CertStoreLocation "Cert:\CurrentUser\My" -KeyUsage DigitalSignature -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.1")
	Write-Host "The certificate has been generated with the name 'Functions Development', and can be found under your Personal certificates in certmgr." -ForegroundColor Green
	
	Write-Host "You should move this certificate to the trusted root store, so that other applications can find and trust it. Do this now? (Y/N) " -ForegroundColor Yellow -NoNewline
	$answer = Read-Host
	if ($answer -eq "Y") { 
		Move-Item -Path $cert.PSPath -Destination "Cert:\CurrentUser\Root"
		Write-Host "The certificate has been added to the Trusted Root Certification Authorities store." -ForegroundColor Green
	}

	Write-Host "Function apps requires the certificate to be exported to a pfx file." -ForegroundColor Yellow
    Write-Host "This file is then referenced in launchSettings.json. Do this export now? (Y/N) " -ForegroundColor Yellow -NoNewline
	$answer = Read-Host

	if ($answer -eq "Y") { 
		Export-PfxCertificate -Cert $cert -FilePath $file -Password (ConvertTo-SecureString -String "123456" -Force -AsPlainText) 
		Write-Host "The certificate file has been exported with password '123456'." -ForegroundColor Green
	}

	Write-Host
	Write-Host "Script done. It might be a good idea to restart your computer now." -ForegroundColor Green

} else {

    Write-Host "Please run this script as administrator to enable certificate generation" -ForegroundColor Red

}