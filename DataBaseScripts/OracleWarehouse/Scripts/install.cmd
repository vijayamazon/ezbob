@echo off
SET /P TNS_NAME="TNS-Alias Name: "
IF "none%TNS_NAME%"=="none" GOTO ERROR
rem SET /P SYSPASS="User 'SYS' password [change_on_istall]: "
rem IF "none%SYSPASS%"=="none" SET SYSPASS=change_on_install
SET /P SYSTEMPASS="User 'SYSTEM' password [manager]: "
IF "none%SYSTEMPASS%"=="none" SET SYSTEMPASS=manager
SET /P SCHEMA_NAME="Destination schema's user name : "
IF "none%SCHEMA_NAME%"=="none" SET SCHEMA_NAME=SSS303 
SET /P TS_TAB="Data tablespace name [TS_SCORTO_TAB]: "
IF "none%TS_TAB%"=="none" SET TS_TAB=TS_SCORTO_TAB
SET /P TS_INDEX="Index tablespace [TS_SCORTO_INDX]: "
IF "none%TS_INDEX%"=="none" SET TS_INDEX=TS_SCORTO_INDX



call gen_run_sql.cmd

sqlplus /nolog @summary.sql %TNS_NAME% %SYSTEMPASS% %SCHEMA_NAME% %TS_TAB% %TS_INDEX%
GOTO EXIT

:ERROR
echo TNS-Alias name is a mandatory parameter!

:EXIT

call clear_runs.cmd

pause

exit