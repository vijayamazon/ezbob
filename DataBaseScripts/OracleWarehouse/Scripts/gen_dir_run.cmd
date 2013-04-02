@echo off
SET FLDR=%1
echo %FLDR%
SET _OUT=.\%FLDR%\runall._sql
echo prompt Process %FLDR% > %_OUT%
for /F "usebackq" %%i in (`dir /B .\%FLDR%\*.sql`) do echo prompt process %%i >> %_OUT% && echo @@%%i >> %_OUT%
rename %_OUT% runall.sql


