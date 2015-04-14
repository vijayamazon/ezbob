IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AV_ReRejectionData]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[AV_ReRejectionData]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AV_GetCustomerBirthDate]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[AV_GetCustomerBirthDate]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AV_GetCustomerMarketPlaces]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[AV_GetCustomerMarketPlaces]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AV_GetCustomerPaymentMarketPlaces]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[AV_GetCustomerPaymentMarketPlaces]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AV_GetAnalysisFunctions]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[AV_GetAnalysisFunctions]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AV_GetAnnualizedRevenueForPeriod]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[AV_GetAnnualizedRevenueForPeriod]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AV_GetExperianScore]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[AV_GetExperianScore]
GO


IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AV_WasLoanApproved]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[AV_WasLoanApproved]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AV_HasDefaultAccounts]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[AV_HasDefaultAccounts]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AV_GetAutomaticDecisions]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[AV_GetAutomaticDecisions]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AV_GetMedalRate]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[AV_GetMedalRate]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AV_IsCustomerOffline]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[AV_IsCustomerOffline]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AV_GetYodleeRevenuesQuarter]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[AV_GetYodleeRevenuesQuarter]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AV_GetYodleeRevenues]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[AV_GetYodleeRevenues]
GO