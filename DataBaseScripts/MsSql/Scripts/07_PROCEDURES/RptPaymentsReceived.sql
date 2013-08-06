IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptPaymentsReceived]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptPaymentsReceived]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE RptPaymentsReceived
@DateStart DATETIME,
@DateEnd   DATETIME,
@ShowNonCashTransactions BIT = NULL
AS
BEGIN
	CREATE TABLE #t (
		PostDate DATETIME NULL,
		LoanID INT NOT NULL,
		ClientID INT NOT NULL,
		ClientEmail NVARCHAR(250) NOT NULL,
		ClientName NVARCHAR(752) NOT NULL,
		Amount NUMERIC(18, 2) NOT NULL,
		LoanRepayment NUMERIC(18, 4) NOT NULL,
		Interest NUMERIC(18, 2) NOT NULL,
		Fees NUMERIC(18, 2) NOT NULL,
		Rollover NUMERIC(18, 4) NOT NULL,
		TransactionType NVARCHAR(8) NOT NULL,
		Description NTEXT,
		SumMatch NVARCHAR(9) NOT NULL,
		RowLevel NVARCHAR(5) NOT NULL
	)

	INSERT INTO #t
	SELECT
		t.PostDate,
		t.LoanId,
		c.Id AS ClientID,
		c.Name AS ClientEmail,
		c.Fullname AS ClientName,
		ISNULL(t.Amount, 0),
		ISNULL(t.LoanRepayment, 0),
		ISNULL(t.Interest, 0),
		ISNULL(t.Fees, 0),
		ISNULL(t.Rollover, 0),
		CASE PaypointId
			WHEN '--- manual ---' THEN 'Manual'
			ELSE 'Paypoint'
		END AS TransactionType,
		t.Description,
		CASE
			WHEN t.LoanRepayment + t.Interest + t.Fees + t.Rollover = t.Amount
				THEN ''
			ELSE 'unmatched'
		END AS SumMatch,
		'' AS RowLevel
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id
		INNER JOIN LoanTransactionMethod m ON t.LoanTransactionMethodId = m.Id
	WHERE
		CONVERT(DATE, @DateStart) <= t.PostDate AND t.PostDate < CONVERT(DATE, @DateEnd)
		AND
		t.Status = 'Done'
		AND
		c.IsTest = 0
		AND
		t.Type = 'PaypointTransaction'
		AND
		(
			(@ShowNonCashTransactions = 0 AND m.Name != 'Non-Cash')
			OR
			@ShowNonCashTransactions != 0
		)

	INSERT INTO #t
	SELECT
		NULL,
		COUNT(DISTINCT LoanID),
		ISNULL(COUNT(DISTINCT ClientID), 0),
		'' AS ClientEmail,
		'Total' AS ClientName,
		ISNULL(SUM(Amount), 0),
		ISNULL(SUM(LoanRepayment), 0),
		ISNULL(SUM(Interest), 0),
		ISNULL(SUM(Fees), 0),
		ISNULL(SUM(Rollover), 0),
		'',
		'',
		'' AS SumMatch,
		'total' AS RowLevel
	FROM
		#t
	WHERE
		RowLevel = ''

	SELECT
		PostDate,
		LoanId,
		ClientID,
		ClientEmail,
		ClientName,
		Amount,
		LoanRepayment,
		Interest,
		Fees,
		Rollover,
		TransactionType,
		Description,
		SumMatch,
		RowLevel
	FROM
		#t
	ORDER BY
		RowLevel DESC,
		PostDate
END
GO
