IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetAvailableStrategyTypes]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[GetAvailableStrategyTypes]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_AssignEmbedded]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Strategy_AssignEmbedded]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_DeletePublishName]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Strategy_DeletePublishName]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_DeletePubRel]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Strategy_DeletePubRel]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_GetDelStrategy]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Strategy_GetDelStrategy]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_NodeStrategyRelCheck]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Strategy_NodeStrategyRelCheck]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_StrategyCheckIn]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Strategy_StrategyCheckIn]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_StrategyInsert]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Strategy_StrategyInsert]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_StrategyList]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Strategy_StrategyList]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_StrategyParamInsert]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Strategy_StrategyParamInsert]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_StrategyPublish]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Strategy_StrategyPublish]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_StrategyRePublish]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Strategy_StrategyRePublish]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_StrategySelect]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Strategy_StrategySelect]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_UpdateChampionStatus]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Strategy_UpdateChampionStatus]
GO


IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetExperianData]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[GetExperianData]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetExperianDataAgentConfigs]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[GetExperianDataAgentConfigs]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetExperianDefaultAccountId]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[GetExperianDefaultAccountId]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetMaxServiceLogId]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[GetMaxServiceLogId]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateExperianFirstIdToHandle]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[UpdateExperianFirstIdToHandle]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetMonitoredSites]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[GetMonitoredSites]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetMonthlyTurnoverPerCashRequest]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[GetMonthlyTurnoverPerCashRequest]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetNdx]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[GetNdx]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_StrategyParamInsert]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Strategy_StrategyParamInsert]
GO


IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_SchemaInsert]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Strategy_SchemaInsert]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_SchemaSelect]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Strategy_SchemaSelect]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetNodeFilesForRestoreAndSave]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[GetNodeFilesForRestoreAndSave]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InsertStrategyCustomerUpdateTime]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[InsertStrategyCustomerUpdateTime]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InsertStrategyMarketPlaceUpdateTime]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[InsertStrategyMarketPlaceUpdateTime]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Nodes_GetDeletedList]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Nodes_GetDeletedList]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Nodes_GetShortList]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Nodes_GetShortList]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_GetAssignedStrategies]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Strategy_GetAssignedStrategies]
GO







IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_GetPublishNames]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Strategy_GetPublishNames]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_GetPublishNamesCount]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Strategy_GetPublishNamesCount]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_GetShortAsgStrategies]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Strategy_GetShortAsgStrategies]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_NodeDelete]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Strategy_NodeDelete]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_NodeInsert]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Strategy_NodeInsert]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_NodeStrategyRelDrop]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Strategy_NodeStrategyRelDrop]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_PublicSignInsert]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Strategy_PublicSignInsert]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_UpdatePublicRel]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Strategy_UpdatePublicRel]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_UpdatePublishName]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Strategy_UpdatePublishName]
GO






















IF OBJECT_ID('BusinessEntity_StrategyRel') IS NOT NULL
BEGIN
	DROP TABLE BusinessEntity_StrategyRel
END
GO
IF OBJECT_ID('BusinessEntity_NodeRel') IS NOT NULL
BEGIN
	DROP TABLE BusinessEntity_NodeRel
END
GO
IF OBJECT_ID('BusinessEntity') IS NOT NULL
BEGIN
	DROP TABLE BusinessEntity
END
GO


IF OBJECT_ID('ExperianDataAgentConfigs') IS NOT NULL
BEGIN
	DROP TABLE ExperianDataAgentConfigs
END
GO
IF OBJECT_ID('MonitoredSites') IS NOT NULL
BEGIN
	DROP TABLE MonitoredSites
END
GO
IF OBJECT_ID('StrategyAccountRel') IS NOT NULL
BEGIN
	DROP TABLE StrategyAccountRel
END
GO
IF OBJECT_ID('Strategy_StrategyParameter') IS NOT NULL
BEGIN
	DROP TABLE Strategy_StrategyParameter
END
GO
IF OBJECT_ID('Strategy_Strategy') IS NOT NULL
BEGIN
	DECLARE @DropStatement NVARCHAR(MAX)
	DECLARE cur CURSOR FOR 
		SELECT 
			'ALTER TABLE ' +  OBJECT_SCHEMA_NAME(parent_object_id) +
			'.[' + OBJECT_NAME(parent_object_id) + 
			'] DROP CONSTRAINT ' + name AS DropStatement
		FROM sys.foreign_keys
		WHERE referenced_object_id = object_id('Strategy_Strategy')
	OPEN cur
	FETCH NEXT FROM cur INTO @DropStatement
	WHILE @@FETCH_STATUS = 0
	BEGIN
		EXECUTE(@DropStatement)

		FETCH NEXT FROM cur INTO @DropStatement
	END
	CLOSE cur
	DEALLOCATE cur
	DROP TABLE Strategy_Strategy
END
GO
IF OBJECT_ID('Strategy_Schemas') IS NOT NULL
BEGIN
	DROP TABLE Strategy_Schemas
END
GO
IF OBJECT_ID('Strategy_Schedule') IS NOT NULL
BEGIN
	DROP TABLE Strategy_Schedule
END
GO
IF OBJECT_ID('Strategy_PublicSign') IS NOT NULL
BEGIN
	DROP TABLE Strategy_PublicSign
END
GO
IF OBJECT_ID('Strategy_PublicRel') IS NOT NULL
BEGIN
	DROP TABLE Strategy_PublicRel
END
GO
IF OBJECT_ID('Strategy_PublicName') IS NOT NULL
BEGIN
	DROP TABLE Strategy_PublicName
END
GO
IF OBJECT_ID('Strategy_ParameterType') IS NOT NULL
BEGIN
	DROP TABLE Strategy_ParameterType
END
GO
IF OBJECT_ID('Strategy_NodeStrategyRel') IS NOT NULL
BEGIN
	DROP TABLE Strategy_NodeStrategyRel
END
GO
IF OBJECT_ID('Strategy_NodeGroup') IS NOT NULL
BEGIN
	DECLARE @DropStatement NVARCHAR(MAX)
	DECLARE cur CURSOR FOR 
		SELECT 
			'ALTER TABLE ' +  OBJECT_SCHEMA_NAME(parent_object_id) +
			'.[' + OBJECT_NAME(parent_object_id) + 
			'] DROP CONSTRAINT ' + name AS DropStatement
		FROM sys.foreign_keys
		WHERE referenced_object_id = object_id('Strategy_NodeGroup')
	OPEN cur
	FETCH NEXT FROM cur INTO @DropStatement
	WHILE @@FETCH_STATUS = 0
	BEGIN
		EXECUTE(@DropStatement)

		FETCH NEXT FROM cur INTO @DropStatement
	END
	CLOSE cur
	DEALLOCATE cur
	DROP TABLE Strategy_NodeGroup
END
GO
IF OBJECT_ID('Strategy_Node') IS NOT NULL
BEGIN
	DROP TABLE Strategy_Node
END
GO
IF OBJECT_ID('Strategy_MarketPlaceUpdateHistory') IS NOT NULL
BEGIN
	DROP TABLE Strategy_MarketPlaceUpdateHistory
END
GO
IF OBJECT_ID('Strategy_Embededrel') IS NOT NULL
BEGIN
	DROP TABLE Strategy_Embededrel
END
GO
IF OBJECT_ID('Strategy_CustomerUpdateHistory') IS NOT NULL
BEGIN
	DROP TABLE Strategy_CustomerUpdateHistory
END
GO
IF OBJECT_ID('Strategy_CalendarRelation') IS NOT NULL
BEGIN
	DROP TABLE Strategy_CalendarRelation
END
GO

























