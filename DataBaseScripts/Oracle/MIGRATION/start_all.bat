@echo off
SET /P TNS_NAME="TNS-Alias Name: "
IF "none%TNS_NAME%"=="none" GOTO ERROR

SET /P SCHEMA_NAME="Destination schema's user name : "


sqlplus %SCHEMA_NAME%/%SCHEMA_NAME%@%TNS_NAME%  @disable_constraints.sql

for /F "usebackq" %%i in (`dir /B *.ctl`) do  sqlldr %SCHEMA_NAME%/%SCHEMA_NAME%@%TNS_NAME% %%i 

sqlplus %SCHEMA_NAME%/%SCHEMA_NAME%@%TNS_NAME%  @InstallOracleASPNETCommon.sql
sqlplus %SCHEMA_NAME%/%SCHEMA_NAME%@%TNS_NAME%  @InstallOracleSessionState.sql
sqlplus %SCHEMA_NAME%/%SCHEMA_NAME%@%TNS_NAME%  @enable_constraints.sql

