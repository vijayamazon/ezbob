IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('Customer') AND name = 'BrokerID')
BEGIN
	ALTER TABLE Customer ADD BrokerID INT NULL
	ALTER TABLE Customer ADD CONSTRAINT FK_CustomerBroker FOREIGN KEY (BrokerID) REFERENCES Broker(BrokerID)
END
GO
