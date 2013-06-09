IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptPaymentsReceived]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptPaymentsReceived]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE RptPaymentsReceived
@DateStart DATETIME,
@DateEnd DATETIME
AS
SELECT
	t.PostDate,
	t.LoanId,
	c.Id AS ClientID,
	c.Name AS ClientEmail,
	c.FirstName + ' ' + c.MiddleInitial + ' ' + c.Surname AS ClientName,
	t.Amount,
	t.LoanRepayment,
	t.Interest,
	t.Fees,
	t.Rollover,
	CASE PaypointId WHEN '--- manual ---' THEN 'Manual' ELSE 'Paypoint' END AS TransactionType,
	t.Description,
	CASE
		WHEN t.LoanRepayment + t.Interest + t.Fees + t.Rollover = t.Amount
			THEN ''
		ELSE 'unmatched'
	END AS SumMatch
FROM
	LoanTransaction t
	INNER JOIN Loan l ON t.LoanId = l.Id
	INNER JOIN Customer c ON l.CustomerId = c.Id
WHERE
	t.PostDate BETWEEN @DateStart AND @DateEnd
	AND
	t.Status = 'Done'
	AND
	c.IsTest = 0
	AND
	t.Type = 'PaypointTransaction'
ORDER BY
	t.PostDate,
	t.Id
GO
