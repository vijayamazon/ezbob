IF OBJECT_ID (N'dbo.ExceededStrategy') IS NOT NULL
	DROP VIEW dbo.ExceededStrategy
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetExceededStrategy]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[GetExceededStrategy]
GO
IF OBJECT_ID (N'dbo.ExceededApplication') IS NOT NULL
	DROP VIEW dbo.ExceededApplication
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetExceededApplication]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[GetExceededApplication]
GO
IF OBJECT_ID (N'dbo.ApplicationState') IS NOT NULL
	DROP VIEW dbo.ApplicationState
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CheckApplicationState]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[CheckApplicationState]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetHistoryByApplicationId]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[GetHistoryByApplicationId]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetApplicationState]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[GetApplicationState]
GO
IF OBJECT_ID (N'dbo.ApplicationsJournal') IS NOT NULL
	DROP VIEW dbo.ApplicationsJournal
GO
IF OBJECT_ID (N'dbo.APPLICATION_VSDELINQ') IS NOT NULL
	DROP VIEW dbo.APPLICATION_VSDELINQ
GO
IF OBJECT_ID (N'dbo.AllApplications') IS NOT NULL
	DROP VIEW dbo.AllApplications
GO
IF OBJECT_ID (N'dbo.GetAllApplications') IS NOT NULL
	DROP PROCEDURE dbo.GetAllApplications
GO
IF OBJECT_ID (N'dbo.GetAllApplication') IS NOT NULL
	DROP PROCEDURE dbo.GetAllApplication
GO

