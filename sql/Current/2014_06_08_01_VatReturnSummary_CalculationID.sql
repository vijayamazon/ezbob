SET QUOTED_IDENTIFIER ON
GO

DROP INDEX MP_VatReturnSummary.IDX_VatReturnSummary
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'CalculationID' AND id = OBJECT_ID('MP_VatReturnSummary'))
BEGIN
	ALTER TABLE MP_VatReturnSummary ADD CalculationID UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_VatReturnSummary_CalculationID DEFAULT (NEWID())

	ALTER TABLE MP_VatReturnSummary DROP CONSTRAINT DF_VatReturnSummary_CalculationID
END
GO

CREATE INDEX IDX_VatReturnSummary ON MP_VatReturnSummary (
	CustomerID,
	CustomerMarketplaceID,
	CalculationID
) WHERE IsActive = 1
GO
