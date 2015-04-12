IF NOT EXISTS (SELECT * FROM syscolumns WHERE name='BrokerID' AND id=object_id('CardInfo'))
BEGIN
	ALTER TABLE CardInfo ADD BrokerID INT 
	ALTER TABLE CardInfo ADD IsDefault BIT
	ALTER TABLE CardInfo ADD CONSTRAINT FK_CardInfo_Broker FOREIGN KEY (BrokerID) REFERENCES Broker(BrokerID)
END
GO
