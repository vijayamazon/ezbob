IF OBJECT_ID('SuggestedAmount') IS NULL
	CREATE TABLE SuggestedAmount (
		Id BIGINT IDENTITY(1, 1) NOT NULL,
		CustomerId INT NOT NULL,
		UnderwriterId INT NOT NULL,
		CashRequestId BIGINT NOT NULL,
		InsertDate DATETIME NOT NULL,
		Amount DECIMAL(18,6) NOT NULL,
		Method NVARCHAR(30),
		Medal NVARCHAR(30)
		CONSTRAINT PK_SuggestedAmount PRIMARY KEY (Id),
		CONSTRAINT FK_SuggestedAmount_Customer FOREIGN KEY (CustomerId) REFERENCES Customer(Id),
		CONSTRAINT FK_SuggestedAmount_Underwriter FOREIGN KEY (UnderwriterId) REFERENCES Security_User(UserId),
		CONSTRAINT FK_SuggestedAmount_CashRequest FOREIGN KEY (CashRequestId) REFERENCES CashRequests(Id),
	)
GO
