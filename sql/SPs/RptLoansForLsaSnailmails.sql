IF OBJECT_ID('RptLoansForLsaSnailmails') IS NULL
	EXECUTE('CREATE PROCEDURE RptLoansForLsaSnailmails AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptLoansForLsaSnailmails
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		LoanID = l.RefNum,
		clm.Path
	FROM
		Loan l
		INNER JOIN PollenLoans lsa ON l.Id = lsa.LoanID
		INNER JOIN CollectionLog cl ON l.CustomerID = cl.CustomerID AND lsa.LoanID = cl.LoanID
		INNER JOIN CollectionSnailMailMetadata clm ON cl.CollectionLogID = clm.CollectionLogID
END
GO
