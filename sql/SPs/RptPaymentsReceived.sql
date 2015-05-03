IF OBJECT_ID('RptPaymentsReceived') IS NULL
BEGIN
	EXECUTE('CREATE PROCEDURE RptPaymentsReceived AS SELECT 1')
END
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[RptPaymentsReceived] 
	(@DateStart DATETIME,
@DateEnd   DATETIME,
@ShowNonCashTransactions BIT = NULL)
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
		TransactionType NVARCHAR(64) NOT NULL,
		Description NTEXT,
		SumMatch NVARCHAR(20) NOT NULL,
		CustomerStatus NVARCHAR(50),
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
		m.Name AS TransactionType,
		t.Description,
		CASE
			WHEN t.LoanRepayment + t.Interest + t.Fees + t.Rollover = t.Amount
				THEN ''
			ELSE 'unmatched'
		END AS SumMatch,
		cs.Name AS CustomerStatus,
		'' AS RowLevel
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id
		LEFT JOIN CustomerStatuses cs ON cs.Id = c.CollectionStatus
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
			@ShowNonCashTransactions IS NULL
			OR
			(@ShowNonCashTransactions = 0 AND m.Name != 'Non-Cash')
			OR
			@ShowNonCashTransactions = 1
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
		'' AS CustomerStatus,
		'total' AS RowLevel
	FROM
		#t
	WHERE
		RowLevel = ''

	SELECT
		t.PostDate,
		t.LoanId,
		t.ClientID,
		t.ClientEmail,
		t.ClientName,
		t.Amount,
		t.LoanRepayment,
		t.Interest,
		t.Fees,
		t.Rollover,
		t.TransactionType,
		t.Description,
		t.CustomerStatus,
		t.SumMatch,
		t.RowLevel
	FROM
		#t AS t
	ORDER BY
		RowLevel DESC,
		PostDate
		
	DROP TABLE #t	
END

GO
