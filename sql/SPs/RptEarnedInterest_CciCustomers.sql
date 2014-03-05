IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptEarnedInterest_CciCustomers]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptEarnedInterest_CciCustomers]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptEarnedInterest_CciCustomers]
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
		AND c.ConsentToSearch = 1 -- TODO: CciMark when it is added
	INNER JOIN LoanTransaction t
		ON l.Id = t.LoanId
		AND t.Status = 'Done'
		AND t.Type = 'PacnetTransaction'
END
GO
