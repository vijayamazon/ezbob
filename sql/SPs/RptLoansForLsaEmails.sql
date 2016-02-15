IF OBJECT_ID('RptLoansForLsaEmails') IS NULL
	EXECUTE('CREATE PROCEDURE RptLoansForLsaEmails AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptLoansForLsaEmails
AS
BEGIN
	SET NOCOUNT ON;

	SELECT DISTINCT
		LoanID = l.RefNum,
		EmailID = e.Id,
		r.FileName,
		r.BinaryBody,
		r.CreationDate
	FROM
		Loan l
		INNER JOIN PollenLoans lsa ON l.Id = lsa.LoanID
		INNER JOIN EzbobMailNodeAttachRelation e ON l.CustomerID = e.UserID
		INNER JOIN Export_Results r ON e.ExportId = r.Id
	WHERE
		LTRIM(RTRIM(ISNULL(r.FileName, ''))) != ''
		AND
		r.BinaryBody IS NOT NULL
END
GO
