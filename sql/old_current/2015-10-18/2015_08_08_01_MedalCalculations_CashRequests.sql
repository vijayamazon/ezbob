SET QUOTED_IDENTIFIER ON
GO

SET ANSI_NULLS ON
GO

IF EXISTS (SELECT * FROM syscolumns WHERE Id = OBJECT_ID('MedalCalculations') AND name = 'TimestampCounter')
	ALTER TABLE MedalCalculations DROP COLUMN TimestampCounter
GO

IF OBJECT_ID('FK_MedalCalculations_NL_CashRequests') IS NOT NULL
	ALTER TABLE MedalCalculations DROP CONSTRAINT FK_MedalCalculations_NL_CashRequests
GO

IF OBJECT_ID('FK_MedalCalculations_CashRequests') IS NOT NULL
	ALTER TABLE MedalCalculations DROP CONSTRAINT FK_MedalCalculations_CashRequests
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE Id = OBJECT_ID('MedalCalculations') AND name = 'CashRequestID')
	ALTER TABLE MedalCalculations ADD CashRequestID BIGINT NULL
GO

IF 56 = (SELECT xtype FROM syscolumns WHERE Id = OBJECT_ID('MedalCalculations') AND name = 'CashRequestID')
	ALTER TABLE MedalCalculations ALTER COLUMN CashRequestID BIGINT NULL
GO

ALTER TABLE MedalCalculations ADD CONSTRAINT FK_MedalCalculations_CashRequests FOREIGN KEY (CashRequestID) REFERENCES CashRequests (Id)
GO

ALTER TABLE MedalCalculations ADD TimestampCounter ROWVERSION
GO
