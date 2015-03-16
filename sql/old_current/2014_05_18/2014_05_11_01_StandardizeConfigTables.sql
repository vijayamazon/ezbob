IF EXISTS (SELECT * from sys.columns WHERE Name = N'StartScore' and Object_ID = Object_ID(N'LoanOfferMultiplier'))
BEGIN
	EXEC sp_RENAME 'LoanOfferMultiplier.StartScore', 'Start' , 'COLUMN'
END
GO

IF EXISTS (SELECT * from sys.columns WHERE Name = N'EndScore' and Object_ID = Object_ID(N'LoanOfferMultiplier'))
BEGIN
	EXEC sp_RENAME 'LoanOfferMultiplier.EndScore', 'End' , 'COLUMN'
END
GO

IF EXISTS (SELECT * from sys.columns WHERE Name = N'Multiplier' and Object_ID = Object_ID(N'LoanOfferMultiplier'))
BEGIN
	EXEC sp_RENAME 'LoanOfferMultiplier.Multiplier', 'Value' , 'COLUMN'
END
GO




IF EXISTS (SELECT * from sys.columns WHERE Name = N'FromScore' and Object_ID = Object_ID(N'BasicInterestRate'))
BEGIN
	EXEC sp_RENAME 'BasicInterestRate.FromScore', 'Start' , 'COLUMN'
END
GO

IF EXISTS (SELECT * from sys.columns WHERE Name = N'ToScore' and Object_ID = Object_ID(N'BasicInterestRate'))
BEGIN
	EXEC sp_RENAME 'BasicInterestRate.ToScore', 'End' , 'COLUMN'
END
GO

IF EXISTS (SELECT * from sys.columns WHERE Name = N'LoanInterestBase' and Object_ID = Object_ID(N'BasicInterestRate'))
BEGIN
	EXEC sp_RENAME 'BasicInterestRate.LoanInterestBase', 'Value' , 'COLUMN'
END
GO

UPDATE BasicInterestRate SET [End] = 10000000 WHERE [End] = 100000000
UPDATE LoanOfferMultiplier SET [End] = 10000000 WHERE [End] = 100000000
GO

DECLARE @CurrentPrecision TinyInt

SELECT @CurrentPrecision = Numeric_Precision FROM INFORMATION_SCHEMA.COLUMNS IC WHERE TABLE_NAME = 'BasicInterestRate' AND COLUMN_NAME = 'Value'
IF @CurrentPrecision != 18
	ALTER TABLE BasicInterestRate ALTER COLUMN Value DECIMAL(18, 6)

SELECT @CurrentPrecision = Numeric_Precision FROM INFORMATION_SCHEMA.COLUMNS IC WHERE TABLE_NAME = 'LoanOfferMultiplier' AND COLUMN_NAME = 'Value'
IF @CurrentPrecision != 18
	ALTER TABLE LoanOfferMultiplier ALTER COLUMN Value DECIMAL(18, 6)
GO

IF OBJECT_ID('BasicInterestRate_Refill') IS NOT NULL
	DROP PROCEDURE BasicInterestRate_Refill
GO

IF OBJECT_ID('LoanOfferMultiplier_Refill') IS NOT NULL
	DROP PROCEDURE LoanOfferMultiplier_Refill
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetBasicInterestRates]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetBasicInterestRates]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetLoanOfferMultipliers]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetLoanOfferMultipliers]
GO









