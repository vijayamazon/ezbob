@echo off

rem Kill all running EzServiceHost instances
taskkill /IM EzServiceHost.exe

set CUR_DISK=c:

%CUR_DISK%

rem EzServiceHost build output
set SOURCE_PATH=%CUR_DISK%\ezbob\App\EzService\EzServiceHost\bin\Debug

rem Run EzServiceHost from this location
set TARGET_PATH=%CUR_DISK%\temp\EzServiceHost

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

cd \

