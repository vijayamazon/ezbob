IF object_id('CustomerTurnover') IS NULL
BEGIN
	CREATE TABLE CustomerTurnover (
		CustomerTurnoverID INT NOT NULL IDENTITY(1,1),
		CustomerID INT NOT NULL,
		Timestamp DATETIME NOT NULL,
		Turnover DECIMAL(18,6) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_CustomerTurnover PRIMARY KEY (CustomerTurnoverID),
		CONSTRAINT FK_CustomerTurnover_Customer FOREIGN KEY (CustomerID) REFERENCES Customer(Id)
	)
END
GO

IF NOT EXISTS (SELECT 1 FROM CustomerTurnover)
BEGIN
	INSERT INTO CustomerTurnover (CustomerID, Timestamp, Turnover)
	SELECT Id, GreetingMailSentDate, OverallTurnOver FROM Customer WHERE GreetingMailSentDate IS NOT NULL AND OverallTurnOver IS NOT NULL
END
GO