@echo off
if "%3" == "" (
	echo Note: read 'echosign.exe' as 'demo.bat'
)
EchoSoap.exe  %*
