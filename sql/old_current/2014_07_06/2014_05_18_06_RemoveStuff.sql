IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteSignalByAppId]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[DeleteSignalByAppId]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ResetAllSignals]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[ResetAllSignals]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Signal_Insert]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Signal_Insert]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateSignalsSelectingStatus]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[UpdateSignalsSelectingStatus]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ResetSignal]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[ResetSignal]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSignals]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[GetSignals]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSignalParameters]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[GetSignalParameters]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetPrioritySignals]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[GetPrioritySignals]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetMessageBySignalId]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[GetMessageBySignalId]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteSignalById]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[DeleteSignalById]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Application_GetStatus]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Application_GetStatus]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateIsExternal]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[UpdateIsExternal]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExpiredNodeExecutionsList]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[ExpiredNodeExecutionsList]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RestoreApplicationFromSuspend]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[RestoreApplicationFromSuspend]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SuspendApplication]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[SuspendApplication]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SuspendApplicationById]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[SuspendApplicationById]
GO

IF OBJECT_ID('Signal') IS NOT NULL
BEGIN
	DROP TABLE Signal
END
GO
