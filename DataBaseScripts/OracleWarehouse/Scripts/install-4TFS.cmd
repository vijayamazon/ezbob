@echo off
SET TNS_NAME=%1
SET SYSTEMPASS=%2
SET SCHEMA_NAME=%3
SET TS_TAB=TS_SCORTO_TAB
SET TS_INDEX=TS_SCORTO_INDX



call gen_run_sql.cmd

sqlplus /nolog @summary.sql %TNS_NAME% %SYSTEMPASS% %SCHEMA_NAME% %TS_TAB% %TS_INDEX%

call clear_runs.cmd