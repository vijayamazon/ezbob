SET QUOTED_IDENTIFIER ON
GO

SET ANSI_NULLS ON
GO

IF OBJECT_ID('PollenLoans') IS NULL
BEGIN
	CREATE TABLE PollenLoans (
		LoanID INT NOT NULL,
		CONSTRAINT PK_PollenLoans PRIMARY KEY (LoanID),
		CONSTRAINT FK_PollenLoans_Loan FOREIGN KEY (LoanID) REFERENCES Loan (Id)
	)
END
GO
