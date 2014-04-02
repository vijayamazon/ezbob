IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('Broker') AND name = 'BrokerTermsID')
BEGIN
	ALTER TABLE Broker ADD BrokerTermsID INT NULL

	ALTER TABLE Broker ADD CONSTRAINT FK_Broker_Terms FOREIGN KEY (BrokerTermsID) REFERENCES BrokerTerms (BrokerTermsID)
END
GO
