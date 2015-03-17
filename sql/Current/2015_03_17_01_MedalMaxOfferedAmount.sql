SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('MedalCalculationsAV') AND name = 'MaxOfferedLoanAmount')
	ALTER TABLE MedalCalculationsAV ADD MaxOfferedLoanAmount INT NULL
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('MedalCalculations') AND name = 'MaxOfferedLoanAmount')
	ALTER TABLE MedalCalculations ADD MaxOfferedLoanAmount INT NULL
GO
