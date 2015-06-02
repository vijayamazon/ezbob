SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('BrokerMarketingFile') AND name = 'TimestampCounter')
	ALTER TABLE BrokerMarketingFile DROP COLUMN TimestampCounter
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('BrokerMarketingFile') AND name = 'OriginID')
BEGIN
	ALTER TABLE BrokerMarketingFile DROP CONSTRAINT UC_BrokerMarketingFile_ID
	
	ALTER TABLE BrokerMarketingFile DROP CONSTRAINT UC_BrokerMarketingFile_Name

	ALTER TABLE BrokerMarketingFile ADD OriginID INT NOT NULL CONSTRAINT DF_BrokerMarketingFile_Origin DEFAULT (1)

	ALTER TABLE BrokerMarketingFile DROP CONSTRAINT DF_BrokerMarketingFile_Origin

	ALTER TABLE BrokerMarketingFile ADD CONSTRAINT FK_BrokerMarketingFile_Origin FOREIGN KEY (OriginID) REFERENCES CustomerOrigin(CustomerOriginID)

	ALTER TABLE BrokerMarketingFile ADD CONSTRAINT UC_BrokerMarketingFile_ID UNIQUE (FileID, OriginID)

	ALTER TABLE BrokerMarketingFile ADD CONSTRAINT UC_BrokerMarketingFile_Name UNIQUE (FileName, OriginID)
END
GO

ALTER TABLE BrokerMarketingFile ADD TimestampCounter ROWVERSION
GO
