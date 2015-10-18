SET QUOTED_IDENTIFIER ON
GO

SET ANSI_NULLS ON
GO

IF EXISTS (SELECT * FROM syscolumns WHERE Id = OBJECT_ID('MedalCalculationsAV') AND name = 'TimestampCounter')
	ALTER TABLE MedalCalculationsAV DROP COLUMN TimestampCounter
GO

IF OBJECT_ID('FK_MedalCalculationsAV_NL_CashRequests') IS NOT NULL
	ALTER TABLE MedalCalculationsAV DROP CONSTRAINT FK_MedalCalculationsAV_NL_CashRequests
GO

IF OBJECT_ID('FK_MedalCalculationsAV_CashRequests') IS NOT NULL
	ALTER TABLE MedalCalculationsAV DROP CONSTRAINT FK_MedalCalculationsAV_CashRequests
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE Id = OBJECT_ID('MedalCalculationsAV') AND name = 'CashRequestID')
	ALTER TABLE MedalCalculationsAV ADD CashRequestID BIGINT NULL
GO

IF 56 = (SELECT xtype FROM syscolumns WHERE Id = OBJECT_ID('MedalCalculationsAV') AND name = 'CashRequestID')
	ALTER TABLE MedalCalculationsAV ALTER COLUMN CashRequestID BIGINT NULL
GO

ALTER TABLE MedalCalculationsAV ADD CONSTRAINT FK_MedalCalculationsAV_CashRequests FOREIGN KEY (CashRequestID) REFERENCES CashRequests (Id)
GO

ALTER TABLE MedalCalculationsAV ADD TimestampCounter ROWVERSION
GO
