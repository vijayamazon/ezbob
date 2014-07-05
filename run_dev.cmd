@echo off

rem Kill all running EzServiceHost instances 
taskkill /IM EzServiceHost.exe

rem Kill all running iisexpress and iisexpresstray instances
taskkill /IM iisexpress*

rem EzServiceHost build output
set SOURCE_PATH=c:\ezbob\App\EzService\EzServiceHost\bin\Debug

rem Run EzServiceHost from this location
set TARGET_PATH=c:\temp\EzServiceHost

rem Create target location
if not exist %TARGET_PATH% (mkdir %TARGET_PATH%)

rem Remove old EzServiceHost version from running location
cd %TARGET_PATH%
del /Q /F /S *.*

rem Copy new EzServiceHost version to running location
cd %SOURCE_PATH%
copy /Y *.* %TARGET_PATH%

rem Start EzServiceHost minimised
echo service start
start /MIN /D %TARGET_PATH% %TARGET_PATH%\EzServiceHost.exe

rem Start iisexpress minimised
echo iis start
start /MIN /D "C:\Program Files (x86)\IIS Express" iisexpress.exe /config:"%USERPROFILE%\Documents\IISExpress\config\applicationhost.config" /site:"EzBob.Web" /apppool:"Clr4IntegratedAppPool"


cd ../../../../.. 


