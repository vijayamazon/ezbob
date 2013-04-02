@echo off

for /F "usebackq" %%j in (`dir /B *.sql`) do  wrap iname= %%j 

del /f /q *.sql
rename *.plb *.sql