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

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Application_Lock]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Application_Lock]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Application_UnLock]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Application_UnLock]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[App_History_LockEvent]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[App_History_LockEvent]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[App_History_Report]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[App_History_Report]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[App_History_Insert]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[App_History_Insert]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[App_History_ExitEvent]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[App_History_ExitEvent]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[App_History_ComingEvent]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[App_History_ComingEvent]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[App_History_UnlockEvent]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[App_History_UnlockEvent]
GO

