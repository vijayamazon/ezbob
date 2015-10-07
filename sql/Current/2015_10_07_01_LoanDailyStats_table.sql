SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoanDailyStats') IS NULL
BEGIN
	CREATE TABLE LoanDailyStats (
		LoanDailyStatsID BIGINT IDENTITY(1, 1) NOT NULL,
		LoanID INT NOT NULL,
		TheDate DATETIME NOT NULL,
		EarnedInterest DECIMAL(18, 2) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LoanDailyStats PRIMARY KEY (LoanDailyStatsID),
		CONSTRAINT FK_LoanDailyStats_Loan FOREIGN KEY (LoanID) REFERENCES Loan (Id),
		CONSTRAINT UC_LoanDailyStats UNIQUE (LoanID, TheDate)
	)
END
GO
