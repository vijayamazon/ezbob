IF EXISTS (SELECT * FROM sysobjects WHERE name = 'CHK_QOffCfg_Enabled')
	ALTER TABLE QuickOfferConfiguration DROP CONSTRAINT CHK_QOffCfg_Enabled
GO

ALTER TABLE QuickOfferConfiguration DROP CONSTRAINT DF_QOffCfg_Enabled
ALTER TABLE QuickOfferConfiguration ALTER COLUMN Enabled INT NOT NULL
ALTER TABLE QuickOfferConfiguration ADD CONSTRAINT DF_QOffCfg_Enabled DEFAULT (1) FOR Enabled
ALTER TABLE QuickOfferConfiguration ADD CONSTRAINT CHK_QOffCfg_Enabled CHECK (Enabled IN (0, 1, 2))
GO
