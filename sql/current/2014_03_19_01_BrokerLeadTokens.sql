IF OBJECT_ID('BrokerLeadTokens') IS NULL
BEGIN
	CREATE TABLE BrokerLeadTokens (
		BrokerLeadTokenID INT IDENTITY(1, 1) NOT NULL,
		BrokerLeadID INT NOT NULL,
		BrokerLeadToken UNIQUEIDENTIFIER NOT NULL,
		DateCreated DATETIME NOT NULL,
		DateDeleted DATETIME NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_BrokerLeadTokens PRIMARY KEY (BrokerLeadTokenID),
		CONSTRAINT FK_BrokerLeadTokens_Lead FOREIGN KEY (BrokerLeadID) REFERENCES BrokerLeads (BrokerLeadID)
	)
END
GO
