SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('CustomerBrokerHistory') IS NULL
	CREATE TABLE CustomerBrokerHistory (
		EntryID BIGINT IDENTITY(1, 1) NOT NULL,
		CustomerID INT NOT NULL,
		FromBrokerID INT NULL,
		ToBrokerID INT NULL,
		EventTime DATETIME NOT NULL,
		UnderwriterID INT NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_CustomerBrokerHistory PRIMARY KEY (EntryID),
		CONSTRAINT FK_CustomerBrokerHistory_Customer FOREIGN KEY (CustomerID) REFERENCES Customer (Id),
		CONSTRAINT FK_CustomerBrokerHistory_FromBroker FOREIGN KEY (FromBrokerID) REFERENCES Broker (BrokerID),
		CONSTRAINT FK_CustomerBrokerHistory_ToBroker FOREIGN KEY (ToBrokerID) REFERENCES Broker (BrokerID),
		CONSTRAINT FK_CustomerBrokerHistory_Underwriter FOREIGN KEY (UnderwriterID) REFERENCES Security_User (UserId)
	)
GO
