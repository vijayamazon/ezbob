IF TYPE_ID('LoanIdListTable') IS NULL
	CREATE TYPE LoanIdListTable AS TABLE(
		LoanID INT NOT NULL
	)
GO

