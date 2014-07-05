IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetNewCustomers]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[GetNewCustomers]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Insert_Security_Role]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Insert_Security_Role]
GO



IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSecirityRolesByUserId]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[GetSecirityRolesByUserId]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSupportAgentConfigs]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[GetSupportAgentConfigs]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetUserFrom]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[GetUserFrom]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetUserTo]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[GetUserTo]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateRole]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[UpdateRole]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LockBegin]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[LockBegin]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LockCheckStatus]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[LockCheckStatus]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LockRelease]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[LockRelease]
GO





IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Log_TraceLogInsert]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Log_TraceLogInsert]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_GeAnnualSalesAmazon]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[MP_GeAnnualSalesAmazon]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_GeAnnualSalesEBay]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[MP_GeAnnualSalesEBay]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_GetAllCustomerMPUMI]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[MP_GetAllCustomerMPUMI]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_GetAmazonFeedback]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[MP_GetAmazonFeedback]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_GetAmazonItemsOrdered]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[MP_GetAmazonItemsOrdered]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_GetAmazonOrdersCancellationRate]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[MP_GetAmazonOrdersCancellationRate]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_GetAmazonOrdersCancelled]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[MP_GetAmazonOrdersCancelled]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_GetAmazonOrdersNumber]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[MP_GetAmazonOrdersNumber]
GO









IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_GetAmazonOrdersTotalSum]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[MP_GetAmazonOrdersTotalSum]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_GetEbayFeedback]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[MP_GetEbayFeedback]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_GetEbayItemsOrdered]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[MP_GetEbayItemsOrdered]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_GetEbayOrdersCancellationRate]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[MP_GetEbayOrdersCancellationRate]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_GetEbayOrdersCancelled]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[MP_GetEbayOrdersCancelled]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_GetEbayOrdersNumber]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[MP_GetEbayOrdersNumber]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_GetInventoryCashValue]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[MP_GetInventoryCashValue]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_GetEbayOrdersTotalSum]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[MP_GetEbayOrdersTotalSum]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_GetInventoryItemsCount]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[MP_GetInventoryItemsCount]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_GetScoreCardData]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[MP_GetScoreCardData]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_IsCustomerMarketPlacesUpdated]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[MP_IsCustomerMarketPlacesUpdated]
GO











IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_UpdateEliminationWarning]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[MP_UpdateEliminationWarning]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_UpdateMPEliminationStatus]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[MP_UpdateMPEliminationStatus]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Paging_Cursor]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Paging_Cursor]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PAGING_CURSOR_COUNT]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[PAGING_CURSOR_COUNT]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Security_ChangePassword]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Security_ChangePassword]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Security_GetSession]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Security_GetSession]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Security_GetUserById]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Security_GetUserById]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Security_GetUserByLogin]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Security_GetUserByLogin]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Security_GrandServiceAction]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[Security_GrandServiceAction]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateExperianBWA_AML]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[UpdateExperianBWA_AML]
GO










IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateLastFoundCustomerId]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[UpdateLastFoundCustomerId]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateMPErrorCustomer]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[UpdateMPErrorCustomer]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateMPErrorMP]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[UpdateMPErrorMP]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateOnTimeCollection]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[UpdateOnTimeCollection]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateTwoWeeksDueMailSent]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[UpdateTwoWeeksDueMailSent]
GO







IF OBJECT_ID('SupportAgentConfigs') IS NOT NULL
BEGIN
	DROP TABLE SupportAgentConfigs
END
GO
























