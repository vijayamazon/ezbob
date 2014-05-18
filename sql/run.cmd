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

	%MY_PATH%Views_2
) do @( call %RUN_ONE_DIR% %RUN_ONE_DIR% %BASH% %SCRIPT% %%d )

echo.
echo.
echo.

echo.
echo Output files:
echo.

dir /A-H-S-D /S /B output*.*

echo.
echo Output files - end of list.
echo.

