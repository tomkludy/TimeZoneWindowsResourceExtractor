####################################################################################
##
## Use for installing all language packs on Windows 11 enterprise
##
## 1. Start with clean Win11 enterprise, all updates installed, no software
## 2. Mount the LOF DVD on D: (or change the $LIPContent variable)
##    - This should be something like:
##      22000.1.210604-1628.co_release_amd64fre_CLIENT_LOF_PACKAGES_OEM.iso
## 3. Run this script from an elevated PowerShell session
##    - It can take hours
## 4. Run Windows Update again and verify all languages are installed
##
## Based on instructions located at:
## https://docs.microsoft.com/en-us/azure/virtual-desktop/windows-11-language-packs
##
####################################################################################

##Disable Language Pack Cleanup##
Disable-ScheduledTask -TaskPath "\Microsoft\Windows\AppxDeploymentClient\" -TaskName "Pre-staged app cleanup"
Disable-ScheduledTask -TaskPath "\Microsoft\Windows\MUI\" -TaskName "LPRemove"
Disable-ScheduledTask -TaskPath "\Microsoft\Windows\LanguageComponentsInstaller" -TaskName "Uninstallation"
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Control Panel\International" /v "BlockCleanupOfUnusedPreinstalledLangPacks" /t REG_DWORD /d 1 /f

##Set Language Pack Content Stores##
$LIPContent = "D:\LanguagesAndOptionalFeatures"

##Get existing user language list, we will add to it
$LanguageList = Get-WinUserLanguageList

##Install every client language pack
Get-ChildItem $LIPContent -Filter Microsoft-Windows-Client-Language-Pack_x64_*.cab | Foreach-Object {
  $fullname = $_.FullName
  Dism /Online /Add-Package /PackagePath:$fullname
  $targetlanguage = $_.BaseName.Substring("Microsoft-Windows-Client-Language-Pack_x64_".Length)
  $LanguageList.Add("$targetlanguage")
}

##Install every language interface pack
Get-ChildItem $LIPContent -Filter Microsoft-Windows-Lip-Language-Pack_x64_*.cab | Foreach-Object {
  $fullname = $_.FullName
  Dism /Online /Add-Package /PackagePath:$fullname
  $targetlanguage = $_.BaseName.Substring("Microsoft-Windows-Lip-Language-Pack_x64_".Length)
  $LanguageList.Add("$targetlanguage")
}

##Update the user language list with all the languages
Set-WinUserLanguageList $LanguageList -force

