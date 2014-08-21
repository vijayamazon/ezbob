SET QUOTED_IDENTIFIER ON
GO

DECLARE @query NVARCHAR(MAX)

IF EXISTS (SELECT * FROM syscolumns WHERE name = 'UserID' AND id = OBJECT_ID('Broker'))
BEGIN
	SET @query = 'BEGIN TRANSACTION' + CHAR(13) + CHAR(10) +
'GO' + CHAR(13) + CHAR(10) +

'ALTER TABLE Broker DROP CONSTRAINT FK_Broker_User' + CHAR(13) + CHAR(10) +
'GO' + CHAR(13) + CHAR(10) +

'ALTER TABLE Broker ADD OldBrokerId INT NULL' + CHAR(13) + CHAR(10) +
'GO' + CHAR(13) + CHAR(10) +

'UPDATE Broker SET OldBrokerId = BrokerID' + CHAR(13) + CHAR(10) +
'GO' + CHAR(13) + CHAR(10) +

'ALTER TABLE Customer DROP CONSTRAINT FK_CustomerBroker' + CHAR(13) + CHAR(10) +
'GO' + CHAR(13) + CHAR(10) +

'UPDATE Customer SET' + CHAR(13) + CHAR(10) +
	'BrokerID = Broker.UserID' + CHAR(13) + CHAR(10) +
'FROM' + CHAR(13) + CHAR(10) +
	'Customer' + CHAR(13) + CHAR(10) +
	'INNER JOIN Broker ON Broker.BrokerID = Customer.BrokerID' + CHAR(13) + CHAR(10) +
'GO' + CHAR(13) + CHAR(10) +

'ALTER TABLE BrokerLeads DROP CONSTRAINT FK_BrokerLead_Broker' + CHAR(13) + CHAR(10) +
'GO' + CHAR(13) + CHAR(10) +

'UPDATE BrokerLeads SET' + CHAR(13) + CHAR(10) +
	'BrokerID = Broker.UserID' + CHAR(13) + CHAR(10) +
'FROM' + CHAR(13) + CHAR(10) +
	'BrokerLeads' + CHAR(13) + CHAR(10) +
	'INNER JOIN Broker ON BrokerLeads.BrokerID = Broker.BrokerID' + CHAR(13) + CHAR(10) +
'GO' + CHAR(13) + CHAR(10) +

'ALTER TABLE CustomerBrokerHistory DROP CONSTRAINT FK_CustomerBrokerHistory_ToBroker' + CHAR(13) + CHAR(10) +
'GO' + CHAR(13) + CHAR(10) +

'ALTER TABLE CustomerBrokerHistory DROP CONSTRAINT FK_CustomerBrokerHistory_FromBroker' + CHAR(13) + CHAR(10) +
'GO' + CHAR(13) + CHAR(10) +

'UPDATE CustomerBrokerHistory SET' + CHAR(13) + CHAR(10) +
	'FromBrokerID = Broker.UserID' + CHAR(13) + CHAR(10) +
'FROM' + CHAR(13) + CHAR(10) +
	'CustomerBrokerHistory' + CHAR(13) + CHAR(10) +
	'INNER JOIN Broker ON CustomerBrokerHistory.FromBrokerID = Broker.BrokerID' + CHAR(13) + CHAR(10) +
'GO' + CHAR(13) + CHAR(10) +

'UPDATE CustomerBrokerHistory SET' + CHAR(13) + CHAR(10) +
	'ToBrokerID = Broker.UserID' + CHAR(13) + CHAR(10) +
'FROM' + CHAR(13) + CHAR(10) +
	'CustomerBrokerHistory' + CHAR(13) + CHAR(10) +
	'INNER JOIN Broker ON CustomerBrokerHistory.ToBrokerID = Broker.BrokerID' + CHAR(13) + CHAR(10) +
'GO' + CHAR(13) + CHAR(10) +

'ALTER TABLE Broker DROP CONSTRAINT PK_Broker' + CHAR(13) + CHAR(10) +
'GO' + CHAR(13) + CHAR(10) +

'ALTER TABLE Broker DROP COLUMN BrokerId' + CHAR(13) + CHAR(10) +
'GO' + CHAR(13) + CHAR(10) +

'EXEC sp_RENAME ''Broker.UserID'', ''BrokerID'', ''COLUMN''' + CHAR(13) + CHAR(10) +
'GO' + CHAR(13) + CHAR(10) +

'ALTER TABLE Broker ADD CONSTRAINT PK_Broker PRIMARY KEY (BrokerID)' + CHAR(13) + CHAR(10) +
'GO' + CHAR(13) + CHAR(10) +

'ALTER TABLE Customer ADD CONSTRAINT FK_Customer_Broker FOREIGN KEY (BrokerId) REFERENCES Broker(BrokerID)' + CHAR(13) + CHAR(10) +
'GO' + CHAR(13) + CHAR(10) +

'ALTER TABLE BrokerLeads ADD CONSTRAINT FK_BrokerLeads_Broker FOREIGN KEY (BrokerId) REFERENCES Broker(BrokerID)' + CHAR(13) + CHAR(10) +
'GO' + CHAR(13) + CHAR(10) +

'ALTER TABLE CustomerBrokerHistory ADD CONSTRAINT FK_CustomerBrokerHistory_ToBroker FOREIGN KEY (ToBrokerId) REFERENCES Broker(BrokerID)' + CHAR(13) + CHAR(10) +
'GO' + CHAR(13) + CHAR(10) +

'ALTER TABLE CustomerBrokerHistory ADD CONSTRAINT FK_CustomerBrokerHistory_FromBroker FOREIGN KEY (FromBrokerId) REFERENCES Broker(BrokerID)' + CHAR(13) + CHAR(10) +
'GO' + CHAR(13) + CHAR(10) +

'ALTER TABLE Broker ADD CONSTRAINT FK_Broker_User FOREIGN KEY (BrokerID) REFERENCES Security_User (UserId)' + CHAR(13) + CHAR(10) +
'GO' + CHAR(13) + CHAR(10) +

'COMMIT TRANSACTION' + CHAR(13) + CHAR(10) +
'GO' + CHAR(13) + CHAR(10)

	EXECUTE(@query)
END
GO
