IF OBJECT_ID (N'dbo.fnInspector_GetTuskListOld') IS NOT NULL
	DROP FUNCTION dbo.fnInspector_GetTuskListOld
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[App_DetailInsertNewNames]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[App_DetailInsertNewNames]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[App_DetailInsertStrategyVar]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[App_DetailInsertStrategyVar]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[App_DetailSelectChildIDs]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[App_DetailSelectChildIDs]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[App_DetailSelectNewNames]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[App_DetailSelectNewNames]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[App_ExecutionPathUpdate]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[App_ExecutionPathUpdate]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StrategyEngine_ExecStatePush]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[StrategyEngine_ExecStatePush]
GO
