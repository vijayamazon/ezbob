@echo off
SET DATABASE_CREATED="0"
SET /P SQLSERVER_NAME="Enter SQLServer Name: "
IF "none%SQLSERVER_NAME%"=="none" GOTO ERROR_SQL_SERVER_NAME_EMPTY
SET /P DATABASE_NAME="Destination database name[SSS_3.03]: "
IF "none%DATABASE_NAME%"=="none" SET DATABASE_NAME=SSS_3.03

SET /P WIN_AUTH="TRUSTED CONNECTION? [Y/N] : "
IF /I "%WIN_AUTH%"=="N" GOTO NOT_TRUSTED_CONNECTION
IF /I "%WIN_AUTH%"=="Y" GOTO TRUSTED_CONNECTION

:NOT_TRUSTED_CONNECTION
SET /P SYSTEMUSER="User 'SQLServer Administartor' name [Sssserver]: "
IF "none%SYSTEMUSER%"=="none" SET SYSTEMUSER=Sssserver
SET /P SYSTEMPWD="User 'SQLServer Administartor' password [sss]: "
IF "none%SYSTEMPWD%"=="none" SET SYSTEMPWD=sss


@echo ============================START==================================
@echo "Creating database section..."
sqlcmd -U %SYSTEMUSER% -P %SYSTEMPWD% -S %SQLSERVER_NAME% -v DATABASE_NAME=%DATABASE_NAME% ERROR_CODE=SQLCMDERRORLEVEL -b -I -i SYS_TASKS\create_database.sql
IF ERRORLEVEL 1 GOTO ERROR
SET DATABASE_CREATED="1"

@echo "Database section executed..."
@echo "=========================================================================="
@echo "Creating tables section..."
FOR %%f IN (02_TABLES\*.sql) DO SQLCMD -U %SYSTEMUSER% -P%SYSTEMPWD% -S %SQLSERVER_NAME% -d %DATABASE_NAME% -b -I -i "%%f"
    IF ERRORLEVEL 1 GOTO ERROR_NOT_TRUSTED

@echo "Tables section executed..."

@echo "=========================================================================="
@echo "Creating triggers section..."
FOR %%f IN (.\09_TRIGGERS\*.sql) DO SQLCMD -U %SYSTEMUSER% -P%SYSTEMPWD% -S %SQLSERVER_NAME% -d %DATABASE_NAME% -b -I -i "%%f"
    IF ERRORLEVEL 1 GOTO ERROR_NOT_TRUSTED

@echo Triggers section executed...
@echo ==========================================================================



@echo "=========================================================================="
@echo "Creating tables constraints and field description section..."
FOR %%f IN (.\10_CONSTRAINTS\*.sql) DO SQLCMD -U %SYSTEMUSER% -P%SYSTEMPWD% -S %SQLSERVER_NAME% -d %DATABASE_NAME% -b -I -i "%%f"
    IF ERRORLEVEL 1 GOTO ERROR_NOT_TRUSTED

@echo "Tables constraints & field description section executed..."



@echo ==========================================================================
@echo "Creating view section..."
FOR %%f IN (.\04_VIEWS\*.sql) DO SQLCMD -U %SYSTEMUSER% -P%SYSTEMPWD% -S %SQLSERVER_NAME% -d %DATABASE_NAME% -b -I -i "%%f"
    IF ERRORLEVEL 1 GOTO ERROR_NOT_TRUSTED
@echo "Views section executed..."



@echo ==========================================================================
@echo "Creating stored procedures section..."
FOR %%f IN (.\07_PROCEDURES\*.sql) DO SQLCMD -U %SYSTEMUSER% -P%SYSTEMPWD% -S %SQLSERVER_NAME% -d %DATABASE_NAME% -b -I -i "%%f"
    IF ERRORLEVEL 1 GOTO ERROR_NOT_TRUSTED
FOR %%f IN (.\07_PROCEDURES\Rank_2\*.sql) DO SQLCMD -U %SYSTEMUSER% -P%SYSTEMPWD% -S %SQLSERVER_NAME% -d %DATABASE_NAME% -b -I -i "%%f"
    IF ERRORLEVEL 1 GOTO ERROR_NOT_TRUSTED
FOR %%f IN (.\07_PROCEDURES\Rank_3\*.sql) DO SQLCMD -U %SYSTEMUSER% -P%SYSTEMPWD% -S %SQLSERVER_NAME% -d %DATABASE_NAME% -b -I -i "%%f"
    IF ERRORLEVEL 1 GOTO ERROR_NOT_TRUSTED

@echo "Stored procedures section executed..."
@echo ==========================================================================

@echo "Creating functions section..."
FOR %%f IN (.\08_FUNCTIONS\*.sql) DO SQLCMD -U %SYSTEMUSER% -P%SYSTEMPWD% -S %SQLSERVER_NAME% -d %DATABASE_NAME% -b -I -i "%%f"
   IF ERRORLEVEL 1 GOTO ERROR_NOT_TRUSTED

@echo "Stored functions executed..."
@echo ==========================================================================

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



@echo "=========================================================================="
@echo "Creating triggers section..."
FOR %%f IN (.\09_TRIGGERS\*.sql) DO SQLCMD -S %SQLSERVER_NAME% -b -I -i "%%f"
   IF ERRORLEVEL 1 GOTO ERROR_TRUSTED
@echo Triggers section executed...



@echo ==========================================================================
@echo Creating tables constraints and field description section..."
FOR %%f IN (.\10_CONSTRAINTS\*.sql) DO SQLCMD -S %SQLSERVER_NAME% -b -I -i "%%f"
IF ERRORLEVEL 1 GOTO ERROR_TRUSTED

@echo "Tables constraints & field description section executed..."



@echo ==========================================================================
@echo "Creating view section..."
FOR %%f IN (.\04_VIEWS\*.sql) DO SQLCMD -S %SQLSERVER_NAME% -b -I -i "%%f"
 IF ERRORLEVEL 1 GOTO ERROR_TRUSTED

@echo "Views section executed..."



@echo ==========================================================================
@echo "Creating stored procedures section..."
FOR %%f IN (.\07_PROCEDURES\*.sql) DO SQLCMD -S %SQLSERVER_NAME% -b -I -i "%%f"
   IF ERRORLEVEL 1 GOTO ERROR_TRUSTED
FOR %%f IN (.\07_PROCEDURES\Rank_2\*.sql) DO SQLCMD -S %SQLSERVER_NAME% -b -I -i "%%f"
    IF ERRORLEVEL 1 GOTO ERROR_TRUSTED
FOR %%f IN (.\07_PROCEDURES\Rank_3\*.sql) DO SQLCMD -S %SQLSERVER_NAME% -b -I -i "%%f"
    IF ERRORLEVEL 1 GOTO ERROR_TRUSTED

@echo "Stored procedures section executed..."
@echo ==========================================================================


@echo "Creating functions section..."
FOR %%f IN (.\08_FUNCTIONS\*.sql) DO SQLCMD -S %SQLSERVER_NAME% -b -I -i "%%f"
   IF ERRORLEVEL 1 GOTO ERROR_TRUSTED

@echo "Stored functions executed..."
@echo ==========================================================================


GOTO EXIT


:ERROR_SQL_SERVER_NAME_EMPTY
@echo SQL Server name must be not empty!
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