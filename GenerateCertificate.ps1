#Check if administrator
if ([Security.Principal.WindowsIdentity]::GetCurrent().Groups -contains 'S-1-5-32-544') {
	#Create / overwrite certificate
	cd $PSScriptRoot
	$cert = New-SelfSignedCertificate -Subject localhost -DnsName localhost -FriendlyName "Functions Development" -KeyUsage DigitalSignature -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.1")
	Export-PfxCertificate -Cert $cert -FilePath certificate.pfx -Password (ConvertTo-SecureString -String "123456" -Force -AsPlainText)
} else {
	#Run this script as administrator
	$script = $PSScriptRoot + "\GenerateCertificate.ps1"
	Start-Process -FilePath PowerShell -ArgumentList $script -Verb RunAs
}