@echo off

set MY_PATH=%~dp0

set BASH=%MY_PATH%Bash\bash.exe

set RUN_ONE_DIR=%MY_PATH%run-one-dir.cmd

set SCRIPT=%MY_PATH%incremental.sh

echo.
echo Executing update database structure from %MY_PATH%...
echo.

for %%d in (
	%MY_PATH%types
	%MY_PATH%current
	%MY_PATH%Triggers
	%MY_PATH%Functions
	%MY_PATH%Views
	%MY_PATH%SPs
) do (
	echo Executing %RUN_ONE_DIR% %RUN_ONE_DIR% %BASH% %SCRIPT% %%d...

	call %RUN_ONE_DIR% %RUN_ONE_DIR% %BASH% %SCRIPT% %%d

	if %ERRORLEVEL% NEQ 0 (
		echo Executing %RUN_ONE_DIR% %RUN_ONE_DIR% %BASH% %SCRIPT% %%d failed.
		goto :LoopExitPoint
	)

	echo Executing %RUN_ONE_DIR% %RUN_ONE_DIR% %BASH% %SCRIPT% %%d complete.
)

:LoopExitPoint

echo.

echo Executing update database structure from %MY_PATH% complete.

echo.

echo.
echo Output files:
echo.

dir /A-H-S-D /S /B output*.*

if %ERRORLEVEL% EQU 0 (
	echo.
	echo Output files found, something went wrong, please examine and fix.
	echo.
	exit /B 1
) else (
	echo.
	echo No output files found, everything went smooth.
	echo.
	exit /B 0
)


