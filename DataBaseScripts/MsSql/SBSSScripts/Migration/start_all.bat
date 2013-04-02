@echo off

SET /P USER_NAME="User name [sa]: "
IF "none%USER_NAME%"=="none" SET USER_NAME=sa

SET /P USER_PASSWD="User password [sa]: "
IF "none%USER_PASSWD%"=="none" SET USER_PASSWD=sa

SET /P SERVER_NAME="Server name [QADB\TEST]: "
IF "none%SERVER_NAME%"=="none" SET SERVER_NAME=QADB\TEST

SET /P DB_NAME="Database name [SBSSTest]: "
IF "none%DB_NAME%"=="none" SET DB_NAME=SBSSTest

SET /P L_PATH="Path to input files ["%CD%"]: "
IF "none%L_PATH%"=="none" SET L_PATH=%CD%

rem sqlcmd /U %USER_NAME% /P %USER_PASSWD% /S %SERVER_NAME% /d %DB_NAME% /i disable_cons.sql 

bcp [%DB_NAME%].[dbo].[CreditApproval500Mixed] in %L_PATH%\CreditApproval500Mixed.txt -S %SERVER_NAME% -U %USER_NAME% -P %USER_PASSWD% -c -w -E -t#

rem sqlcmd /U %USER_NAME% /P %USER_PASSWD% /S %SERVER_NAME% /d %DB_NAME% /i enable_cons.sql 

rem exit 
pause

:ERROR
exit