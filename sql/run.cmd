@echo off

set MY_PATH=%~dp0

set BASH=%MY_PATH%Bash\bash.exe

set SCRIPT=%MY_PATH%incremental.sh

set QUERIES_PATH=%MY_PATH%current
set SPS_PATH=%MY_PATH%SPs
set VIEWS_PATH=%MY_PATH%Views

%BASH% %SCRIPT% %QUERIES_PATH%

%BASH% %SCRIPT% %VIEWS_PATH%

%BASH% %SCRIPT% %SPS_PATH%

