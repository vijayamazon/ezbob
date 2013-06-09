IF OBJECT_ID('LoanScheduleTransaction') IS NULL
BEGIN
	CREATE TABLE LoanScheduleTransaction (
		ID BIGINT IDENTITY NOT NULL,
		LoanID INT NOT NULL,
		ScheduleID INT NOT NULL,
		TransactionID INT NOT NULL,
		Date DATETIME NOT NULL CONSTRAINT DF_LoanScheduleTransaction DEFAULT (GETDATE()),
		PrincipalDelta NUMERIC(18, 2) NOT NULL,
		FeesDelta NUMERIC(18, 2) NOT NULL,
		InterestDelta NUMERIC(18, 2) NOT NULL,
		StatusBefore NVARCHAR (50) NOT NULL,
		StatusAfter NVARCHAR (50) NOT NULL,
		CONSTRAINT PK_LoanScheduleTransaction PRIMARY KEY (ID),
		CONSTRAINT FK_LoanST_Loan FOREIGN KEY (LoanID) REFERENCES Loan (Id),
		CONSTRAINT FK_LoanST_Schedule FOREIGN KEY (ScheduleID) REFERENCES LoanSchedule (Id),
		CONSTRAINT FK_LoanST_Transaction FOREIGN KEY (TransactionID) REFERENCES LoanTransaction (Id)
	)
END
GO
