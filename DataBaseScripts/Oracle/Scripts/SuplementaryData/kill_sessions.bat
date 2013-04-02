@echo off
SET /P TNS_NAME="TNS-Alias Name: "
IF "none%TNS_NAME%"=="none" GOTO ERROR
SET /P SYSTEMPASS="User 'SYSTEM' password : "

SET /P SCHEMA_NAME="Destination schema's user name : "
sqlplus system/%SYSTEMPASS%@%TNS_NAME% @kill_session.sql %SCHEMA_NAME%

pause
