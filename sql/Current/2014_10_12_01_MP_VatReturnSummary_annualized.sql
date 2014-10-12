SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'AnnualizedTurnover' AND id = OBJECT_ID('MP_VatReturnSummary'))
	ALTER TABLE MP_VatReturnSummary ADD AnnualizedTurnover DECIMAL(18, 6) NULL
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'AnnualizedValueAdded' AND id = OBJECT_ID('MP_VatReturnSummary'))
	ALTER TABLE MP_VatReturnSummary ADD AnnualizedValueAdded DECIMAL(18, 6) NULL
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'AnnualizedFreeCashFlow' AND id = OBJECT_ID('MP_VatReturnSummary'))
	ALTER TABLE MP_VatReturnSummary ADD AnnualizedFreeCashFlow DECIMAL(18, 6) NULL
GO
