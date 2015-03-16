IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fnGetNodesFromPatch]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP FUNCTION [dbo].[fnGetNodesFromPatch]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IntToTime]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP FUNCTION [dbo].[IntToTime]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_GetCurrencyRate]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP FUNCTION [dbo].[MP_GetCurrencyRate]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_GetCurrencyRate1]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP FUNCTION [dbo].[MP_GetCurrencyRate1]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_List]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP FUNCTION [dbo].[MP_List]
GO


IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetMarketPlaceStatus]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP FUNCTION [dbo].[GetMarketPlaceStatus]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetMarketPlaceStatusByName]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP FUNCTION [dbo].[GetMarketPlaceStatusByName]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetTableFromList]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP FUNCTION [dbo].[GetTableFromList]
GO



IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetExpensesPayPalTransactionsByPayer]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[GetExpensesPayPalTransactionsByPayer]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetIncomePayPalTransactionsByPayer]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[GetIncomePayPalTransactionsByPayer]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetTotalExpensesPayPalTransactions]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[GetTotalExpensesPayPalTransactions]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetTotalIncomePayPalTransactions]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[GetTotalIncomePayPalTransactions]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetTotalTransactionsPayPalTransactions]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[GetTotalTransactionsPayPalTransactions]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_GetAmazonOrdersTotalSum]') AND TYPE IN (N'P', N'PC', N'FN', N'TF'))
DROP PROCEDURE [dbo].[MP_GetAmazonOrdersTotalSum]
GO





IF OBJECT_ID('CommandsList') IS NOT NULL
BEGIN
	DROP TABLE CommandsList
END
GO
IF OBJECT_ID('Commands') IS NOT NULL
BEGIN
	DROP TABLE Commands
END
GO
IF OBJECT_ID('Log_ServiceAction') IS NOT NULL
BEGIN
	DROP TABLE Log_ServiceAction
END
GO
IF OBJECT_ID('Log_TraceLog') IS NOT NULL
BEGIN
	DROP TABLE Log_TraceLog
END
GO
IF OBJECT_ID('LockedObject') IS NOT NULL
BEGIN
	DROP TABLE LockedObject
END
GO
IF OBJECT_ID('AvailableFunds') IS NOT NULL
BEGIN
	DROP TABLE AvailableFunds
END
GO
IF OBJECT_ID('Control_History') IS NOT NULL
BEGIN
	DROP TABLE Control_History
END
GO
IF OBJECT_ID('EmailAccount') IS NOT NULL
BEGIN
	DROP TABLE EmailAccount
END
GO
IF OBJECT_ID('ExclusiveApplication') IS NOT NULL
BEGIN
	DROP TABLE ExclusiveApplication
END
GO
IF OBJECT_ID('Export_OperationJournal') IS NOT NULL
BEGIN
	DROP TABLE Export_OperationJournal
END
GO
IF OBJECT_ID('Export_TemplateNodeRel') IS NOT NULL
BEGIN
	DROP TABLE Export_TemplateNodeRel
END
GO
IF OBJECT_ID('Export_TemplatesList') IS NOT NULL
BEGIN
	DROP TABLE Export_TemplatesList
END
GO






IF OBJECT_ID('Export_TemplateStratRel') IS NOT NULL
BEGIN
	DROP TABLE Export_TemplateStratRel
END
GO
IF OBJECT_ID('Filter') IS NOT NULL
BEGIN
	DROP TABLE Filter
END
GO
IF OBJECT_ID('IncrementalUpdateLog') IS NOT NULL
BEGIN
	DROP TABLE IncrementalUpdateLog
END
GO
IF OBJECT_ID('Log4Net') IS NOT NULL
BEGIN
	DROP TABLE Log4Net
END
GO
































