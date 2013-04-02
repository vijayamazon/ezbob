@echo off
SET TNS_NAME=%1
SET SCHEMA_NAME=%2


sqlplus %SCHEMA_NAME%/%SCHEMA_NAME%@%TNS_NAME%  @disable_constraints.sql

for /F "usebackq" %%i in (`dir /B *.ctl`) do  sqlldr %SCHEMA_NAME%/%SCHEMA_NAME%@%TNS_NAME% %%i 

sqlplus %SCHEMA_NAME%/%SCHEMA_NAME%@%TNS_NAME%  @enable_constraints.sql

