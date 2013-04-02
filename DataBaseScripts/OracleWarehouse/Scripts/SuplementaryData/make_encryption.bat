@echo off

for /F "usebackq" %%j in (`dir /B *.sql`) do  wrap iname= %%j 

rename *.sql *.sq_
rename *.plb *.sql