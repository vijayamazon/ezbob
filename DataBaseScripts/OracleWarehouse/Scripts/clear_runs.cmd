@echo off
for /F "usebackq" %%j in (`dir /B /A:D`) do echo deleting .\%%j\runall.sql && del .\%%j\runall.sql
rem pause
rem exit
