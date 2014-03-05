IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptEarnedInterest_IssuedLoans]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptEarnedInterest_IssuedLoans]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptEarnedInterest_IssuedLoans] 
	(@DateStart DATE,
@DateEnd DATE)
AS
BEGIN
	SELECT
	l.Id AS LoanID,
	CONVERT(DATE, l.Date) AS IssueDate,
	t.Amount
FROM
	Loan l
	INNER JOIN Customer c
		ON l.CustomerId = c.Id
		AND c.IsTest = 0
	INNER JOIN LoanTransaction t
		ON l.Id = t.LoanId
		AND t.Status = 'Done'
		AND t.Type = 'PacnetTransaction'
WHERE
	@DateStart <= l.Date AND l.Date < @DateEnd
END
GO
