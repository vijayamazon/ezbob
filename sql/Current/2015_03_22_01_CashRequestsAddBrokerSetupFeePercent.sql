SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('CashRequests') AND name = 'BrokerSetupFeePercent')
	ALTER TABLE CashRequests ADD BrokerSetupFeePercent DECIMAL (18, 4) NULL
GO

UPDATE dbo.ConfigurationVariables
SET Value = '100'
WHERE Name = 'SetupFeeFixed'
GO
