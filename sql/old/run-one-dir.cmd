@echo off

set RUN_ONE_DIR=%1

set BASH=%2

set SCRIPT=%3

set CURRENT_PATH=%4

echo.
echo.
echo %CURRENT_PATH%
echo.

%BASH% %SCRIPT% %CURRENT_PATH%

if %ERRORLEVEL% NEQ 0 (
	echo Incremental script failed with code is %ERRORLEVEL% - exiting.
	exit /B 1
)

echo Going for sub-directories of %CURRENT_PATH%...

for /D %%d in (%CURRENT_PATH%\*.*) do (
	echo Processing sub-directory %%d...

	@call %RUN_ONE_DIR% %RUN_ONE_DIR% %BASH% %SCRIPT% %%d

	if %ERRORLEVEL% NEQ 0 (
		echo Sub-directory script failed with code is %ERRORLEVEL% - exiting.
		exit /B 1
	)

	echo Processing sub-directory %%d complete.
)

echo Done for %CURRENT_PATH%.

exit /B 0

