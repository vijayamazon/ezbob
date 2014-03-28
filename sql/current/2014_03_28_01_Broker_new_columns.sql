IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('Broker') AND name = 'FirmWebSiteUrl')
BEGIN
	ALTER TABLE Broker ADD FirmWebSiteUrl NVARCHAR(255) NULL

	ALTER TABLE Broker ADD EstimatedMonthlyApplicationCount INT NOT NULL CONSTRAINT DF_Broker_AppCount DEFAULT(0)

	ALTER TABLE Broker ADD AgreedToTermsDate DATETIME NOT NULL CONSTRAINT DF_Broker_AgreedToTermsDate DEFAULT(GETUTCDATE())

	ALTER TABLE Broker ADD AgreedToPrivacyPolicyDate DATETIME NOT NULL CONSTRAINT DF_Broker_AgreedToPrivacyPolicyDate DEFAULT(GETUTCDATE())
END
GO
