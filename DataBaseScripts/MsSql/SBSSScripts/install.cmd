@echo off
SET DATABASE_CREATED="0"
SET /P SQLSERVER_NAME="Enter SQLServer Name[qadb\test]: "
IF "none%SQLSERVER_NAME%"=="none" SET SQLSERVER_NAME=qadb\test
SET /P DATABASE_NAME="Destination database name[SBSSTest]: "
IF "none%DATABASE_NAME%"=="none" SET DATABASE_NAME=SBSSTest

SET /P WIN_AUTH="TRUSTED CONNECTION? [Y/N] : "
IF /I "%WIN_AUTH%"=="N" GOTO NOT_TRUSTED_CONNECTION
IF /I "%WIN_AUTH%"=="Y" GOTO TRUSTED_CONNECTION

:NOT_TRUSTED_CONNECTION
SET /P SYSTEMUSER="User 'SQLServer Administartor' name [sa]: "
IF "none%SYSTEMUSER%"=="none" SET SYSTEMUSER=sa
SET /P SYSTEMPWD="User 'SQLServer Administartor' password [sa]: "
IF "none%SYSTEMPWD%"=="none" SET SYSTEMPWD=sa


@echo ============================START==================================
@echo "Creating database section..."
sqlcmd -U %SYSTEMUSER% -P %SYSTEMPWD% -S %SQLSERVER_NAME% -v DATABASE_NAME=%DATABASE_NAME% ERROR_CODE=SQLCMDERRORLEVEL -b -I -i SYS_TASKS\create_database.sql
IF ERRORLEVEL 1 GOTO ERROR
SET DATABASE_CREATED="1"
@echo "Database section executed..."

@echo "=========================================================================="
@echo "Creating tables section..."
FOR %%f IN (TABLES\*.sql) DO SQLCMD -U %SYSTEMUSER% -P%SYSTEMPWD% -S %SQLSERVER_NAME% -v DATABASE_NAME=%DATABASE_NAME% -b -I -i "%%f"
    IF ERRORLEVEL 1 GOTO ERROR_NOT_TRUSTED

@echo "Tables section executed..."

GOTO EXIT


:TRUSTED_CONNECTION
@echo ============================START==================================
@echo "Creating database section..."
sqlcmd -S %SQLSERVER_NAME% -v DATABASE_NAME=%DATABASE_NAME% -b -E -I -i SYS_TASKS\create_database.sql
IF ERRORLEVEL 1 GOTO ERROR
SET DATABASE_CREATED="1"
@echo "Database section executed..."

@echo "=========================================================================="
@echo "Creating tables section..."
FOR %%f IN (.\02_TABLES\*.sql) DO SQLCMD -S %SQLSERVER_NAME% -b -I -i "%%f"
IF ERRORLEVEL 1 GOTO ERROR_TRUSTED

@echo Tables section executed...


GOTO EXIT


:ERROR
@echo =====================Error============================
GOTO EXIT

:ERROR_TRUSTED
@echo =================Error============================
@echo Rollback all operations........
sqlcmd -S %SQLSERVER_NAME% -v DATABASE_NAME=%DATABASE_NAME% -b -E -I -i SYS_TASKS\drop_database.sql
GOTO EXIT

:ERROR_NOT_TRUSTED
@echo =================Error============================
@echo Rollback all operations........
sqlcmd -U %SYSTEMUSER% -P%SYSTEMPWD% -S %SQLSERVER_NAME% -v DATABASE_NAME=%DATABASE_NAME% -b -E -I -i SYS_TASKS\drop_database.sql
GOTO EXIT

:EXIT

pause

exit