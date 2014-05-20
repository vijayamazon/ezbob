
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Dump_Application_Variables]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Dump_Application_Variables]
GO




IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[App_RaiseAppNotExistError]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[App_RaiseAppNotExistError]
GO






IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AppDetail_DeleteSubTree]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[AppDetail_DeleteSubTree]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AppDetail_DeleteSubTreeByName]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[AppDetail_DeleteSubTreeByName]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AppDetail_UpdateAttach]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[AppDetail_UpdateAttach]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Application_Attachment_Insert]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Application_Attachment_Insert]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Application_DetailInsert]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Application_DetailInsert]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Application_DetailSelect]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Application_DetailSelect]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BankBranch_Sign_Add]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[BankBranch_Sign_Add]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AddLink]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[AddLink]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Application_DetailUpdate]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Application_DetailUpdate]
GO








IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[APPLICATION_GET_CHILDCOUNT]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[APPLICATION_GET_CHILDCOUNT]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[APPLICATION_GET_PARRENT]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[APPLICATION_GET_PARRENT]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[APPLICATION_GET_STRATEGY]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[APPLICATION_GET_STRATEGY]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[APPLICATION_GET_VERSION]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[APPLICATION_GET_VERSION]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Application_Insert]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Application_Insert]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[APPLICATION_UPDATE_CHILDCOUNT]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[APPLICATION_UPDATE_CHILDCOUNT]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ApplicationDetail_Delete]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[ApplicationDetail_Delete]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ApproveLinksBySourceId]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[ApproveLinksBySourceId]
GO







IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Export_GetEntityLinkSeries]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP FUNCTION [dbo].[Export_GetEntityLinkSeries]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteLinksBySourceId]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[DeleteLinksBySourceId]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Export_AddExportResult]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Export_AddExportResult]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Export_GetExportResults]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Export_GetExportResults]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Export_GetTemplates]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Export_GetTemplates]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetEntityLink]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[GetEntityLink]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetEntityLinks]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[GetEntityLinks]
GO


IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreateDump]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[CreateDump]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Get_Application_Results]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Get_Application_Results]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Save_Application_Results]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Save_Application_Results]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetExceededNodes]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[GetExceededNodes]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetNodeDataJournal]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[GetNodeDataJournal]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ErrorMessageSave]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[ErrorMessageSave]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetElementId]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[GetElementId]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetStoredParametersByAppId]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[GetStoredParametersByAppId]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetAttachmentByParentDetail]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[GetAttachmentByParentDetail]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ChangeLockedUser]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[ChangeLockedUser]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_CheckStrategyState]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Strategy_CheckStrategyState]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_GetStrategyInfo]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Strategy_GetStrategyInfo]
GO











IF OBJECT_ID('App_Attach_DocType') IS NOT NULL
BEGIN
	DROP TABLE App_Attach_DocType
END
GO
IF OBJECT_ID('Application_VariablesDumpData') IS NOT NULL
BEGIN
	DROP TABLE Application_VariablesDumpData
END
GO
IF OBJECT_ID('Application_VariablesDump') IS NOT NULL
BEGIN
	DROP TABLE Application_VariablesDump
END
GO
IF OBJECT_ID('Application_Suspended') IS NOT NULL
BEGIN
	DROP TABLE Application_Suspended
END
GO
IF OBJECT_ID('Application_Setting') IS NOT NULL
BEGIN
	DROP TABLE Application_Setting
END
GO
IF OBJECT_ID('Application_Result') IS NOT NULL
BEGIN
	DROP TABLE Application_Result
END
GO
IF OBJECT_ID('Application_NodeTime') IS NOT NULL
BEGIN
	DROP TABLE Application_NodeTime
END
GO
IF OBJECT_ID('Application_NodeSetting') IS NOT NULL
BEGIN
	DROP TABLE Application_NodeSetting
END
GO
IF OBJECT_ID('Application_NodeDataSign') IS NOT NULL
BEGIN
	DROP TABLE Application_NodeDataSign
END
GO
IF OBJECT_ID('Application_HistorySumm') IS NOT NULL
BEGIN
	DROP TABLE Application_HistorySumm
END
GO
IF OBJECT_ID('Application_History') IS NOT NULL
BEGIN
	DROP TABLE Application_History
END
GO
IF OBJECT_ID('Application_ExecutionType') IS NOT NULL
BEGIN
	DROP TABLE Application_ExecutionType
END
GO
IF OBJECT_ID('Application_Error_History') IS NOT NULL
BEGIN
	DROP TABLE Application_Error_History
END
GO
IF OBJECT_ID('Application_Error') IS NOT NULL
BEGIN
	DROP TABLE Application_Error
END
GO
IF OBJECT_ID('Application_DetailName') IS NOT NULL
BEGIN
	
	DECLARE @DropStatement NVARCHAR(MAX)
	DECLARE cur CURSOR FOR 
		SELECT 
			'ALTER TABLE ' +  OBJECT_SCHEMA_NAME(parent_object_id) +
			'.[' + OBJECT_NAME(parent_object_id) + 
			'] DROP CONSTRAINT ' + name AS DropStatement
		FROM sys.foreign_keys
		WHERE referenced_object_id = object_id('Application_DetailName')
	OPEN cur
	FETCH NEXT FROM cur INTO @DropStatement
	WHILE @@FETCH_STATUS = 0
	BEGIN
		EXECUTE(@DropStatement)

		FETCH NEXT FROM cur INTO @DropStatement
	END
	CLOSE cur
	DEALLOCATE cur
	DROP TABLE Application_DetailName
END
GO
IF OBJECT_ID('Application_Detail') IS NOT NULL
BEGIN
	DECLARE @DropStatement NVARCHAR(MAX)
	DECLARE cur CURSOR FOR 
		SELECT 
			'ALTER TABLE ' +  OBJECT_SCHEMA_NAME(parent_object_id) +
			'.[' + OBJECT_NAME(parent_object_id) + 
			'] DROP CONSTRAINT ' + name AS DropStatement
		FROM sys.foreign_keys
		WHERE referenced_object_id = object_id('Application_Detail')
	OPEN cur
	FETCH NEXT FROM cur INTO @DropStatement
	WHILE @@FETCH_STATUS = 0
	BEGIN
		EXECUTE(@DropStatement)

		FETCH NEXT FROM cur INTO @DropStatement
	END
	CLOSE cur
	DEALLOCATE cur
	DROP TABLE Application_Detail
END
GO
IF OBJECT_ID('Application_Attachment') IS NOT NULL
BEGIN
	DROP TABLE Application_Attachment
END
GO
IF OBJECT_ID('Application_Application') IS NOT NULL
BEGIN
	DROP TABLE Application_Application
END
GO

IF OBJECT_ID('EntityLink') IS NOT NULL
BEGIN
	DROP TABLE EntityLink
END
GO
IF OBJECT_ID('BankBranch_Sign') IS NOT NULL
BEGIN
	DROP TABLE BankBranch_Sign
END
GO






