rem @echo off

SET USER_NAME=%2
SET USER_PASSWD=%3
SET SERVER_NAME=%1
SET DB_NAME=%4

rem SET /P L_PATH="Path to input files [D:\test\sss_3.03]: "
rem IF "none%L_PATH%"=="none" SET L_PATH=D:\test\sss_3.03

sqlcmd /U %USER_NAME% /P %USER_PASSWD% /S %SERVER_NAME% /d %DB_NAME% /i disable_cons.sql 

bcp [%DB_NAME%].[dbo].[Security_user] in Security_user.txt -S %SERVER_NAME% -U %USER_NAME% -P %USER_PASSWD% -c -w -E -t#
bcp [%DB_NAME%].[dbo].[Security_Role] in Security_Role.txt -S %SERVER_NAME% -U %USER_NAME% -P %USER_PASSWD% -c -w -E -t#
bcp [%DB_NAME%].[dbo].[Security_Application] in Security_Application.txt -S %SERVER_NAME% -U %USER_NAME% -P %USER_PASSWD% -c -w -E -t#
bcp [%DB_NAME%].[dbo].[Security_UserRoleRelation] in Security_UserRoleRelation.txt -S %SERVER_NAME% -U %USER_NAME% -P %USER_PASSWD% -c -w -E -t#
bcp [%DB_NAME%].[dbo].[Strategy_NodeGroup] in Strategy_NodeGroup.txt -S %SERVER_NAME% -U %USER_NAME% -P %USER_PASSWD% -c -w -E -t#
bcp [%DB_NAME%].[dbo].[Strategy_ParameterType] in Strategy_ParameterType.txt -S %SERVER_NAME% -U %USER_NAME% -P %USER_PASSWD% -c -w -E -t#
bcp [%DB_NAME%].[dbo].[App_Attach_DocType] in App_Attach_DocType.txt -S %SERVER_NAME% -U %USER_NAME% -P %USER_PASSWD% -c -w -E -t#
bcp [%DB_NAME%].[dbo].[Application_DetailName] in Application_DetailName.txt -S %SERVER_NAME% -U %USER_NAME% -P %USER_PASSWD% -c -w -E -t#
bcp [%DB_NAME%].[dbo].[StrategyAreas] in StrategyAreas.txt -S %SERVER_NAME% -U %USER_NAME% -P %USER_PASSWD% -c -w -E -t#
bcp [%DB_NAME%].[dbo].[StrategyTasks] in StrategyTasks.txt -S %SERVER_NAME% -U %USER_NAME% -P %USER_PASSWD% -c -w -E -t#
bcp [%DB_NAME%].[dbo].[AppStatus] in AppStatus_LM.txt -S %SERVER_NAME% -U %USER_NAME% -P %USER_PASSWD% -c -w -E -t#
bcp [%DB_NAME%].[dbo].[Security_RoleAppRel] in Security_RoleAppRel.txt -S %SERVER_NAME% -U %USER_NAME% -P %USER_PASSWD% -c -w -E -t#
bcp [%DB_NAME%].[dbo].[MenuItem] in MenuItem.txt -S %SERVER_NAME% -U %USER_NAME% -P %USER_PASSWD% -c -w -E -t#
bcp [%DB_NAME%].[dbo].[hibernate_unique_key] in hibernate_unique_key.txt -S %SERVER_NAME% -U %USER_NAME% -P %USER_PASSWD% -c -w -E -t#
bcp [%DB_NAME%].[dbo].[Security_Question] in SecurityQuestion.txt -S %SERVER_NAME% -U %USER_NAME% -P %USER_PASSWD% -c -w -E -t#
bcp [%DB_NAME%].[dbo].[DBString] in DBString.txt -S %SERVER_NAME% -U %USER_NAME% -P %USER_PASSWD% -c -w -E -t#
bcp [%DB_NAME%].[dbo].[CaisFlags] in CaisFlags.txt -S %SERVER_NAME% -U %USER_NAME% -P %USER_PASSWD% -c -w -E -t#
bcp [%DB_NAME%].[dbo].[LoanType] in LoanType.txt -S %SERVER_NAME% -U %USER_NAME% -P %USER_PASSWD% -c -w -E -t#
bcp [%DB_NAME%].[dbo].[Security_Permission] in Security_Permission.txt -S %SERVER_NAME% -U %USER_NAME% -P %USER_PASSWD% -c -w -E -t#
bcp [%DB_NAME%].[dbo].[Security_RolePermissionRel] in Security_RolePermissionRel.txt -S %SERVER_NAME% -U %USER_NAME% -P %USER_PASSWD% -c -w -E -t#
bcp [%DB_NAME%].[dbo].[ConfigurationVariables] in ConfigurationVariables.txt -S %SERVER_NAME% -U %USER_NAME% -P %USER_PASSWD% -c -w -E -t#
bcp [%DB_NAME%].[dbo].[MP_MarketplaceType] in MP_MarketplaceType.txt -S %SERVER_NAME% -U %USER_NAME% -P %USER_PASSWD% -c -w -E -t#

sqlcmd /U %USER_NAME% /P %USER_PASSWD% /S %SERVER_NAME% /d %DB_NAME% /i enable_cons.sql

rem exit 

:ERROR

