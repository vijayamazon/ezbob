SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('MedalCalculations') AND name = 'CapOfferByCustomerScoresValue')
	ALTER TABLE MedalCalculations ADD CapOfferByCustomerScoresValue DECIMAL(18, 6) NULL
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('MedalCalculations') AND name = 'CapOfferByCustomerScoresTable')
	ALTER TABLE MedalCalculations ADD CapOfferByCustomerScoresTable NVARCHAR(MAX) NULL
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('MedalCalculationsAV') AND name = 'CapOfferByCustomerScoresValue')
	ALTER TABLE MedalCalculationsAV ADD CapOfferByCustomerScoresValue DECIMAL(18, 6) NULL
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('MedalCalculationsAV') AND name = 'CapOfferByCustomerScoresTable')
	ALTER TABLE MedalCalculationsAV ADD CapOfferByCustomerScoresTable NVARCHAR(MAX) NULL
GO
