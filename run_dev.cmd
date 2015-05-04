@echo off

rem Kill all running iisexpress and iisexpresstray instances
taskkill /IM iisexpress*

set MY_PATH=%~dp0

call %MY_PATH%run_service.cmd

rem Start iisexpress minimised - App site
echo iis app start
start /MIN /D "C:\Program Files (x86)\IIS Express" iisexpress.exe /config:"%USERPROFILE%\Documents\IISExpress\config\applicationhost.config" /site:"EzBob.Web" /apppool:"Clr4IntegratedAppPool"

rem Start iisexpress minimised - API site
rem echo iis api start
rem start /MIN /D "C:\Program Files (x86)\IIS Express" iisexpress.exe /config:"%USERPROFILE%\Documents\IISExpress\config\applicationhost.config" /site:"Demo" /apppool:"Clr4IntegratedAppPool"

