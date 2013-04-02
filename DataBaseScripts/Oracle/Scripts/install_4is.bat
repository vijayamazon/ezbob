@echo off
SET TNS_NAME=%1
SET SYSTEMPASS=%2
SET SCHEMA_NAME=%3
SET TS_TAB=%4
SET TS_INDEX=%5

call gen_run_sql.cmd

echo %TNS_NAME%
echo %SYSTEMPASS%
echo %SCHEMA_NAME%
echo %TS_TAB%
echo %TS_INDEX%
          
sqlplus /nolog @summary.sql %TNS_NAME% %SYSTEMPASS% %SCHEMA_NAME% %TS_TAB% %TS_INDEX%

call clear_runs.cmd