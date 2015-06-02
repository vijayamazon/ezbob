SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('BrokerTerms') AND name = 'TimestampCounter')
	ALTER TABLE BrokerTerms DROP COLUMN TimestampCounter
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('BrokerTerms') AND name = 'OriginID')
BEGIN
	ALTER TABLE BrokerTerms ADD OriginID INT NOT NULL CONSTRAINT DF_BrokerTerms_Origin DEFAULT(1)
	
	ALTER TABLE BrokerTerms ADD CONSTRAINT FK_BrokerTerms_Origin FOREIGN KEY (OriginID) REFERENCES CustomerOrigin (CustomerOriginID)
	
	ALTER TABLE BrokerTerms DROP CONSTRAINT DF_BrokerTerms_Origin
END
GO

ALTER TABLE BrokerTerms ADD TimestampCounter ROWVERSION
GO
