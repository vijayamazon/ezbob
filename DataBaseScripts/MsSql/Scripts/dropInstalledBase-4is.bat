@echo off
SET DATABASE_CREATED="0"
SET SQLSERVER_NAME=%1
SET DATABASE_NAME=%4

SET WIN_AUTH=N
IF /I "%WIN_AUTH%"=="N" GOTO NOT_TRUSTED_CONNECTION
IF /I "%WIN_AUTH%"=="Y" GOTO TRUSTED_CONNECTION

:NOT_TRUSTED_CONNECTION
SET SYSTEMUSER=%2
SET SYSTEMPWD=%3

@echo ============================drop database==================================
@echo "Drop database section..."
sqlcmd -U %SYSTEMUSER% -P %SYSTEMPWD% -S %SQLSERVER_NAME% -v DATABASE_NAME=%DATABASE_NAME% ERROR_CODE=SQLCMDERRORLEVEL -b -I -i SYS_TASKS\drop_database_new.sql
