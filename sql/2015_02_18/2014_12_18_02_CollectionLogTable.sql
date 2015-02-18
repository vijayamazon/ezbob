SET QUOTED_IDENTIFIER ON
GO

IF object_id('CollectionLog') IS NULL
BEGIN
	CREATE TABLE CollectionLog(
		CollectionLogID INT NOT NULL IDENTITY(1,1),
		CustomerID INT NOT NULL,
		LoanID INT NOT NULL,
		TimeStamp DATETIME NOT NULL,
		Type NVARCHAR(30),
		Method NVARCHAR(30),
		CONSTRAINT PK_CollectionLog PRIMARY KEY (CollectionLogID),
		CONSTRAINT FK_CollectionLog_Customer FOREIGN KEY (CustomerID) REFERENCES Customer(Id),
		CONSTRAINT FK_CollectionLog_Loan FOREIGN KEY (LoanID) REFERENCES Loan(Id)
	)
END
GO
