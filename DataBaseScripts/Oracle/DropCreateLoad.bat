SET TNS_NAME=%1
SET SYSTEMPASS=%2
SET SCHEMA_NAME=%3
SET TS_TAB=TS_SCORTO_TAB
SET TS_INDEX=TS_SCORTO_INDX

SET SCHEMA_PASSWD=%SCHEMA_NAME%

   echo DROPing
cd Scripts\SuplementaryData
   echo DROP all 3 times
rem to be sure make it twice
echo ONE!!!!
call kill_sessions-4TFS.bat %TNS_NAME% %SYSTEMPASS% %SCHEMA_NAME%
call drop_schema-4TFS.bat %TNS_NAME% %SYSTEMPASS% %SCHEMA_NAME%
echo TWO!!!!
call kill_sessions-4TFS.bat %TNS_NAME% %SYSTEMPASS% %SCHEMA_NAME%
call drop_schema-4TFS.bat %TNS_NAME% %SYSTEMPASS% %SCHEMA_NAME%
echo THREE!!!!
call kill_sessions-4TFS.bat %TNS_NAME% %SYSTEMPASS% %SCHEMA_NAME%
call drop_schema-4TFS.bat %TNS_NAME% %SYSTEMPASS% %SCHEMA_NAME%
cd ..\..

   echo CREATE new scema
cd Scripts
call install-4TFS.cmd %TNS_NAME% %SYSTEMPASS% %SCHEMA_NAME%
cd ..

   echo LOAD DATA!
cd MIGRATION
call start_all-4TFS.bat %TNS_NAME% %SCHEMA_NAME%
cd ..