SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('OfferCalculations') AND name = 'GradeID')
	ALTER TABLE OfferCalculations ADD GradeID INT NULL
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('OfferCalculations') AND name = 'SubGradeID')
	ALTER TABLE OfferCalculations ADD SubGradeID INT NULL
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('OfferCalculations') AND name = 'CashRequestID')
	ALTER TABLE OfferCalculations ADD CashRequestID BIGINT NULL
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('OfferCalculations') AND name = 'NLCashRequestID')
	ALTER TABLE OfferCalculations ADD NLCashRequestID BIGINT NULL
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('OfferCalculations') AND name = 'TimestampCounter')
	ALTER TABLE OfferCalculations ADD TimestampCounter ROWVERSION
GO
