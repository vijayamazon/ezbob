IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetLoanOfferMultiplier]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetLoanOfferMultiplier]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetBasicInterestRate]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetBasicInterestRate]
GO
