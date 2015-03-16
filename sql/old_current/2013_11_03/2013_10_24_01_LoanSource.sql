IF OBJECT_ID('LoanSource') IS NULL
BEGIN
	CREATE TABLE LoanSource (
		LoanSourceID INT NOT NULL,
		LoanSourceName NVARCHAR(50) NOT NULL,
		MaxInterest NUMERIC(18, 2) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LoanSource PRIMARY KEY (LoanSourceID),
		CONSTRAINT CHK_LoanSource CHECK (LoanSourceID > 0),
		CONSTRAINT CHK_LoanSource_Name CHECK (LoanSourceName != ''),
		CONSTRAINT CHK_LoanSource_MaxInterest CHECK (MaxInterest IS NULL OR MaxInterest > 0)
	)

	CREATE UNIQUE INDEX IDX_LoanSource_Name ON LoanSource(LoanSourceName)

	INSERT INTO LoanSource (LoanSourceID, LoanSourceName, MaxInterest) VALUES
		(1, 'Standard', NULL),
		(2, 'EU', 0.02)

	ALTER TABLE Loan ADD
		LoanSourceID INT NOT NULL CONSTRAINT DF_Loan_SourceID DEFAULT (1),
		CONSTRAINT FK_Loan_SourceID FOREIGN KEY(LoanSourceID) REFERENCES LoanSource (LoanSourceID)

	ALTER TABLE CashRequests ADD
		LoanSourceID INT NOT NULL CONSTRAINT DF_CashRequest_SourceID DEFAULT (1),
		CONSTRAINT FK_CashRequest_SourceID FOREIGN KEY(LoanSourceID) REFERENCES LoanSource (LoanSourceID)
END
GO
