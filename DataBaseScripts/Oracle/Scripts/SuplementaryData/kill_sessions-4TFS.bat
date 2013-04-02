@echo off
SET TNS_NAME=%1
SET SYSTEMPASS=%2
SET SCHEMA_NAME=%3

sqlplus system/%SYSTEMPASS%@%TNS_NAME% @kill_session.sql %SCHEMA_NAME%
