IF OBJECT_ID('RptEarnedInterest_CciCustomers') IS NULL
	EXECUTE('CREATE PROCEDURE RptEarnedInterest_CciCustomers AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptEarnedInterest_CciCustomers
@DateStart DATE,
@DateEnd DATE
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		l.Id AS LoanID,
		CONVERT(DATE, l.Date) AS IssueDate,
		t.Amount,
		l.CustomerId
	FROM
		Loan l
		INNER JOIN Customer c
			ON l.CustomerId = c.Id
			AND c.IsTest = 0
			AND c.ConsentToSearch = 1 -- TODO: CciMark when it is added
		INNER JOIN LoanTransaction t
			ON l.Id = t.LoanId
			AND t.Status = 'Done'
			AND t.Type = 'PacnetTransaction'
END
GO
