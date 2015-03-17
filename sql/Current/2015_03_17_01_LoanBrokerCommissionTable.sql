IF object_id('LoanBrokerCommission') IS NULL
BEGIN
	CREATE TABLE LoanBrokerCommission (
		LoanBrokerCommissionID INT IDENTITY(1,1),
		LoanID INT NOT NULL,
		BrokerID INT NOT NULL,
		CardInfoID INT NOT NULL,
		CommissionAmount DECIMAL(18,2) NOT NULL,
		CreateDate DATETIME NOT NULL,
		PaidDate DATETIME,
		TrackingNumber NVARCHAR(100),
		Status NVARCHAR(50),
		Description NVARCHAR(100),
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LoanBrokerCommission PRIMARY KEY (LoanBrokerCommissionID),
		CONSTRAINT FK_LoanBrokerCommission_Loan FOREIGN KEY (LoanID) REFERENCES Loan(Id),
		CONSTRAINT FK_LoanBrokerCommission_Broker FOREIGN KEY (BrokerID) REFERENCES Broker(BrokerID),
		CONSTRAINT FK_LoanBrokerCommission_CardInfo FOREIGN KEY (CardInfoID) REFERENCES CardInfo(Id),
	)
END
GO
