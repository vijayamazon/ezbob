IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateCreatorUser]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateCreatorUser]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateAppAttachmentsState]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateAppAttachmentsState]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateApplicationNodeSetting]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateApplicationNodeSetting]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateApplicationStrategyId]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateApplicationStrategyId]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateIsTimeoutReported]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateIsTimeoutReported]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SE_ExecStateStackDepth]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[SE_ExecStateStackDepth]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetReassignApplications]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetReassignApplications]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetLockedApplications]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetLockedApplications]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetLinksForAppOnUserInput]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetLinksForAppOnUserInput]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetExecAppParams]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetExecAppParams]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetChildApplicationsByParentId]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetChildApplicationsByParentId]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetChildApplicationInfo]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetChildApplicationInfo]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GeApplicationById]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GeApplicationById]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetApplicationByAppCounter]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetApplicationByAppCounter]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetApplicationById]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetApplicationById]
GO
