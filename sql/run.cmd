@echo off

set MY_PATH=%~dp0

set BASH=%MY_PATH%Bash\bash.exe

set RUN_ONE_DIR=%MY_PATH%run-one-dir.cmd

set SCRIPT=%MY_PATH%incremental.sh

for %%d in (
	%MY_PATH%types

	%MY_PATH%current
	%MY_PATH%Triggers
	%MY_PATH%Functions
	%MY_PATH%Views
	%MY_PATH%SPs

) do @( call %RUN_ONE_DIR% %RUN_ONE_DIR% %BASH% %SCRIPT% %%d )

echo.
echo.
echo.

echo.
echo Output files:
echo.

dir /A-H-S-D /S /B output*.*
if %ERRORLEVEL% NEQ 0 exit 1


echo.
echo Output files - end of list.
echo.

