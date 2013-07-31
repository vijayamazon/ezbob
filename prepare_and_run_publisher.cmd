cls
@echo off

::variables
set tmpPath=_publisher.tmp
set publisherPath=Tools\publisher
set testDataXlsPath=Items\TestData.xls

::implementations
echo 1. Create directory %tmpPath%
@echo:
md %tmpPath%

echo 2. Copy all files from %publisherPath% into "%tmpPath%"
xcopy "%publisherPath%" "%tmpPath%" /c /d /e /h /i /k /q /r /s /x /y
@echo:

echo 3. Copy TestData.xls from %testDataXlsPath% into "%tmpPath%"
xcopy "%testDataXlsPath%" "%tmpPath%" /c /d /e /h /i /k /q /r /s /x /y
@echo:


IF "%1%"=="" (
	ECHO  4. Run publisher without parameters
) ELSE (
	ECHO 4. Run publisher with parameters %1%
)
cd %tmpPath%
publisher %1%
cd ..
@echo:

echo 5. Remove directory %tmpPath%
rd %tmpPath% /S /Q
@echo: