IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptOverallStats]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptOverallStats]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE RptOverallStats
@DateStart DATETIME,
@DateEnd   DATETIME
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @Money_Given FLOAT = (
		SELECT
			SUM(t.Amount)
		FROM
			LoanTransaction t
			INNER JOIN Loan l ON t.LoanId = l.Id
			INNER JOIN Customer c ON l.CustomerId = c.Id
		WHERE
			t.Status = 'Done'
			AND
			t.Type = 'PacnetTransaction'
			AND
			c.IsTest = 0
	)

	DECLARE @Money_Repaid FLOAT = (
		SELECT
			SUM(t.Principal)
		FROM
			LoanTransaction t
			INNER JOIN Loan l ON t.LoanId = l.Id
			INNER JOIN Customer c ON l.CustomerId = c.Id
		WHERE
			t.Status = 'Done'
			AND
			t.Type = 'PaypointTransaction'
			AND
			c.IsTest = 0
	)

	DECLARE @Money_Out FLOAT = @Money_Given - @Money_Repaid

	-- DATEDIFF returns number of months since begining of time.
	-- The inner DATEADD creates the date "the first day of the next month".
	DECLARE @NextMonthStart DATETIME = (
		DATEADD(month,
			DATEDIFF(month, 0, GETDATE()) + 1,
		0)
	)
	-- SQL Server 2012 contains EOMONTH function.

	SELECT
		y.LineId,
		y.Name,
		y.Value
	FROM (
		SELECT
			11 LineId,
			'Total Anual Shop Revenue that where given loans' Name,
			-- Высокие... высокие отношения!
			PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), SUM(v.ValueFloat))), 1), 2) Value
		FROM
			MP_AnalyisisFunctionValues v
			INNER JOIN MP_AnalyisisFunction f ON v.AnalyisisFunctionId = f.Id AND f.Name = 'TotalSumOfOrders'
			INNER JOIN MP_AnalysisFunctionTimePeriod t ON v.AnalysisFunctionTimePeriodId = t.Id AND t.Name = '365'

		UNION

		SELECT
			10 LineId,
			'Total Money to be Repaid Until End of Month' Name,
			-- Не менее высокие отношения...
			PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), SUM(s.AmountDue))), 1), 2) Value
		FROM
			LoanSchedule s
			INNER JOIN Loan l ON s.LoanId = l.Id
			INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
		WHERE
			s.Status = 'StillToPay'
			AND
			GETDATE() <= s.[Date] AND s.[Date] < @NextMonthStart

		UNION

		SELECT
			2 LineId,
			'Total Money Given' Name,
			PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), @Money_Given)), 1), 2) Value

		UNION

		SELECT
			3 LineId,
			'Total Money Repaid' Name,
			PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), @Money_Repaid)), 1), 2) Value

		UNION

		SELECT
			1 LineId,
			'Total Money Out' Name,
			PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), @Money_Out)), 1), 2) Value

		UNION

		SELECT
			7 LineId,
			'Setup Fee' Name,
			PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), SUM(ISNULL(Fees, 0)))), 1), 2) Value
		FROM
			LoanTransaction
		WHERE
			Type = 'PacnetTransaction'
			AND
			Status = 'Done'

		UNION

		SELECT
			5 LineId,
			'Interest Back' Name,
			PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), SUM(LoanTransaction.Interest))), 1), 2) Value
		FROM
			LoanTransaction
			INNER JOIN Loan l ON LoanTransaction.LoanId = l.Id
			INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
		WHERE
			LoanTransaction.Type = 'PaypointTransaction'
			AND
			LoanTransaction.Status = 'Done'

		UNION

		SELECT
			4 LineId,
			'Principal Back' Name,
			PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), SUM(LoanTransaction.LoanRepayment))), 1), 2) Value
		FROM
			LoanTransaction
			INNER JOIN Loan l ON LoanTransaction.LoanId = l.Id
			INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
		WHERE
			LoanTransaction.Type = 'PaypointTransaction'
			AND
			LoanTransaction.Status = 'Done'

		UNION

		SELECT
			6 LineId,
			'Fees Back' Name,
			PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), SUM(LoanTransaction.Fees))), 1), 2) Value
		FROM
			LoanTransaction
			INNER JOIN Loan l ON LoanTransaction.LoanId = l.Id
			INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
		WHERE
			LoanTransaction.Type = 'PaypointTransaction'
			AND
			LoanTransaction.Status = 'Done'

		UNION

		SELECT
			8 LineId,
			'Late Money' Name,
			PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), SUM(x.Amount))), 1), 2) Value
		FROM (
			SELECT
				LoanSchedule.LoanId,
				MAX(LoanSchedule.AmountDue) Amount
			FROM
				LoanSchedule
				INNER JOIN Loan l ON LoanSchedule.LoanId = l.Id
				INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
			WHERE
				(LoanSchedule.Status='StillToPay' AND LoanSchedule.[Date] < GETDATE())
				OR
				(LoanSchedule.Status='Late' AND LoanSchedule.RepaymentAmount = 0)
			GROUP BY
				LoanSchedule.LoanId
		) x

		UNION

		SELECT
			9 LineId,
			'Late Principal' Name,
			PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), SUM(x.Amount))), 1), 2) Value
		FROM (
			SELECT
				LoanSchedule.LoanId,
				max(LoanSchedule.LoanRepayment) Amount
			FROM
				LoanSchedule
				INNER JOIN Loan l ON LoanSchedule.LoanId = l.Id
				INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
			WHERE
				(LoanSchedule.Status = 'StillToPay' AND LoanSchedule.[Date] < GETDATE())
				OR
				(LoanSchedule.Status = 'Late' AND LoanSchedule.RepaymentAmount = 0)
			GROUP BY
				LoanId
		) x
	) y
	ORDER BY
		y.LineId
END
GO
