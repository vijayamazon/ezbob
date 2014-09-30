IF OBJECT_ID('RptPaymentReport') IS NULL
	EXECUTE('CREATE PROCEDURE RptPaymentReport AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptPaymentReport
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	SELECT
		ls.Id,
		c.FirstName + ' ' + c.Surname AS Name,
		c.Name AS Email,
		ls.[Date],
		AmountDue
	FROM
		LoanSchedule ls
		INNER JOIN Loan l ON l.Id = ls.LoanId
		INNER JOIN Customer c ON c.Id = l.CustomerId
	WHERE
		ls.Status = 'StillToPay'
		AND
		c.IsTest = 0
		AND
		@DateStart <= ls.[Date] AND ls.[Date] < @DateEnd
	ORDER BY
		ls.[Date]
END
GO
