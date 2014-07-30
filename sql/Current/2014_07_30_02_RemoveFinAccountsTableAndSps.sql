IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeletePreviousFinancialAccounts]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[DeletePreviousFinancialAccounts]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InsertFinancialAccount]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[InsertFinancialAccount]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetUnprocessedServiceLogEntries]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetUnprocessedServiceLogEntries]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetCustomerActiveAccounts]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetCustomerActiveAccounts]
GO


IF (EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FinancialAccounts]') AND type in (N'U')))
BEGIN
	ALTER TABLE Customer DROP CONSTRAINT DF_Customer_FinancialAccounts
	ALTER TABLE Customer DROP COLUMN FinancialAccounts
	DROP TABLE FinancialAccounts
END 
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetDirectorsScore]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetDirectorsScore]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetExperianConsumerCacheDate]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetExperianConsumerCacheDate]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetExperianScore]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetExperianScore]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateExperianConsumer]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateExperianConsumer]
GO
