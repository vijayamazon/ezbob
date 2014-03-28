@echo off

set RUN_ONE_DIR=%1

set BASH=%2

set SCRIPT=%3

set CURRENT_PATH=%4

echo.
echo.
echo %CURRENT_PATH%

%BASH% %SCRIPT% %CURRENT_PATH%

for /D %%d in (%CURRENT_PATH%\*.*) do @call %RUN_ONE_DIR% %RUN_ONE_DIR% %BASH% %SCRIPT% %%d

