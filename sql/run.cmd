@echo off

set MY_PATH=%~dp0

set BASH=%MY_PATH%Bash\bash.exe

set SCRIPT=%MY_PATH%incremental.sh

set QUERIES_PATH=%MY_PATH%current

"%BASH%" "%SCRIPT%" "%QUERIES_PATH%"

