@echo off
SET TNS_NAME=%1
SET SYSTEMPASS=%2
SET SCHEMA_NAME=%3

sqlplus -S -L system/%SYSTEMPASS%@%TNS_NAME% @drop_schema.sql %SCHEMA_NAME%
