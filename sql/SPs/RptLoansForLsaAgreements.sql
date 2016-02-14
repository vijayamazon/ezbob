SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('RptLoansForLsaAgreements') IS NULL
	EXECUTE('CREATE PROCEDURE RptLoansForLsaAgreements AS SELECT 1')
GO

ALTER PROCEDURE RptLoansForLsaAgreements
AS
BEGIN
	SELECT
		LoanID = ll.RefNum,
		FilePath
	FROM
		LoanAgreement a
		INNER JOIN PollenLoans l ON a.LoanId = l.LoanID
		INNER JOIN Loan ll ON l.LoanID = ll.Id
END
GO
