IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateMPErrorMP]') AND TYPE IN (N'P', N'PC'))
	DROP PROCEDURE [dbo].[UpdateMPErrorMP]
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InsertStrategyMarketPlaceUpdateTime]') AND TYPE IN (N'P', N'PC'))
	DROP PROCEDURE [dbo].[InsertStrategyMarketPlaceUpdateTime]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InsertStrategyCustomerUpdateTime]') AND TYPE IN (N'P', N'PC'))
	DROP PROCEDURE [dbo].[InsertStrategyCustomerUpdateTime]
GO
