SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('CashRequests') AND name = 'TimestampCounter')
	ALTER TABLE CashRequests DROP COLUMN TimestampCounter
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('CashRequests') AND name = 'SpreadSetupFee')
	ALTER TABLE CashRequests ADD SpreadSetupFee BIT NULL
GO

ALTER TABLE CashRequests ADD TimestampCounter ROWVERSION
GO
