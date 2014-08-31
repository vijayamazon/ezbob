SET QUOTED_IDENTIFIER ON
GO

DECLARE @query NVARCHAR(MAX)

IF EXISTS (SELECT * FROM syscolumns WHERE name = 'UserID' AND id = OBJECT_ID('Broker'))
BEGIN
	SET @query = 'ALTER TABLE Broker DROP CONSTRAINT FK_Broker_User'
	EXECUTE(@query)
	
	SET @query = 'ALTER TABLE BrokerInstantOfferRequest DROP CONSTRAINT FK_BrokerInstantOfferRequest_BrokerId'
	EXECUTE(@query)

	SET @query = 'ALTER TABLE Broker ADD OldBrokerId INT NULL' 
	EXECUTE(@query)

	SET @query = 'UPDATE Broker SET OldBrokerId = BrokerID' 
	EXECUTE(@query)

	SET @query = 'ALTER TABLE Customer DROP CONSTRAINT FK_CustomerBroker' 
	EXECUTE(@query)

	SET @query = '
		UPDATE Customer SET 
			BrokerID = Broker.UserID
		FROM 
			Customer INNER JOIN Broker ON Broker.BrokerID = Customer.BrokerID'
	EXECUTE(@query)

	SET @query = 'ALTER TABLE BrokerLeads DROP CONSTRAINT FK_BrokerLead_Broker'
	EXECUTE(@query)

	SET @query = '
		UPDATE BrokerLeads SET
			BrokerID = Broker.UserID
		FROM
			BrokerLeads INNER JOIN Broker ON BrokerLeads.BrokerID = Broker.BrokerID'
	EXECUTE(@query)

	SET @query = 'ALTER TABLE CustomerBrokerHistory DROP CONSTRAINT FK_CustomerBrokerHistory_ToBroker'
	EXECUTE(@query)

	SET @query = 'ALTER TABLE CustomerBrokerHistory DROP CONSTRAINT FK_CustomerBrokerHistory_FromBroker'
	EXECUTE(@query)

	SET @query = '
		UPDATE CustomerBrokerHistory SET
			FromBrokerID = Broker.UserID
		FROM
			CustomerBrokerHistory INNER JOIN Broker ON CustomerBrokerHistory.FromBrokerID = Broker.BrokerID'
	EXECUTE(@query)

	SET @query = '
		UPDATE CustomerBrokerHistory SET
			ToBrokerID = Broker.UserID
		FROM
			CustomerBrokerHistory INNER JOIN Broker ON CustomerBrokerHistory.ToBrokerID = Broker.BrokerID'
	EXECUTE(@query)

	SET @query = 'ALTER TABLE Broker DROP CONSTRAINT PK_Broker'
	EXECUTE(@query)

	SET @query = 'ALTER TABLE Broker DROP COLUMN BrokerId'
	EXECUTE(@query)

	SET @query = 'EXEC sp_RENAME ''Broker.UserID'', ''BrokerID'', ''COLUMN'''
	EXECUTE(@query)

	SET @query = 'ALTER TABLE Broker ADD CONSTRAINT PK_Broker PRIMARY KEY (BrokerID)'
	EXECUTE(@query)
	
	SET @query = 'ALTER TABLE BrokerInstantOfferRequest ADD CONSTRAINT FK_BrokerInstantOfferRequest_BrokerId FOREIGN KEY (BrokerId) REFERENCES Broker(BrokerID)'
	EXECUTE(@query)

	SET @query = 'ALTER TABLE Customer ADD CONSTRAINT FK_Customer_Broker FOREIGN KEY (BrokerId) REFERENCES Broker(BrokerID)'
	EXECUTE(@query)

	SET @query = 'ALTER TABLE BrokerLeads ADD CONSTRAINT FK_BrokerLeads_Broker FOREIGN KEY (BrokerId) REFERENCES Broker(BrokerID)'
	EXECUTE(@query)

	SET @query = 'ALTER TABLE CustomerBrokerHistory ADD CONSTRAINT FK_CustomerBrokerHistory_ToBroker FOREIGN KEY (ToBrokerId) REFERENCES Broker(BrokerID)'
	EXECUTE(@query)

	SET @query = 'ALTER TABLE CustomerBrokerHistory ADD CONSTRAINT FK_CustomerBrokerHistory_FromBroker FOREIGN KEY (FromBrokerId) REFERENCES Broker(BrokerID)'
	EXECUTE(@query)
	
	SET @query = 'ALTER TABLE Broker ADD CONSTRAINT FK_Broker_User FOREIGN KEY (BrokerID) REFERENCES Security_User (UserId)'
	EXECUTE(@query)
END
GO
