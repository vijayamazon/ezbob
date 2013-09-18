IF OBJECT_ID('LoanInterestFreeze') IS NULL
BEGIN
	CREATE TABLE LoanInterestFreeze (
		Id INT IDENTITY(1, 1) NOT NULL,
		LoanId INT NOT NULL,
		StartDate DATETIME NULL,
		EndDate DATETIME NULL,
		InterestRate DECIMAL(18, 4) NOT NULL,
		ActivationDate DATETIME NOT NULL CONSTRAINT DF_LoanInterestFreeze_Active DEFAULT (GETDATE()),
		DeactivationDate DATETIME NULL,
		CONSTRAINT PK_LoanInterestFreeze PRIMARY KEY (Id),
		CONSTRAINT FK_LoanInterestFreeze FOREIGN KEY (LoanId) REFERENCES Loan(Id)
	)
END
GO
