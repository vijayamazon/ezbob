cls
@echo off

::variables
set publisherPath=Tools\publisher
set testDataXlsPath=Items\TestData.xls

::implementations
echo 1. Rename TestData.xls into  _TestData.xls
RENAME  "%publisherPath%\TestData.xls" "_TestData.xls"
@echo:

echo 2. Copy TestData.xls from %testDataXlsPath% into "%publisherPath%"
xcopy "%testDataXlsPath%" "%publisherPath%" /c /d /e /h /i /k /q /r /s /x /y
@echo:




ECHO 3. Run publisher with parameters (--delete)

cd %publisherPath%
publisher.exe --delete
@echo:

echo 2'. Copy TestData.xls from %testDataXlsPath% into "%publisherPath%"
xcopy "%testDataXlsPath%" "%publisherPath%" /c /d /e /h /i /k /q /r /s /x /y
@echo:

ECHO  3'. Run publisher without parameters
publisher.exe
@echo:


echo 4. Delete TestData.xls
DEL TestData.xls
@echo:

echo 5. Rename _TestData.xls to TestData.xls
RENAME _TestData.xls TestData.xls
@echo:

echo 6. Go to initial dir
cd ../..
@echo: