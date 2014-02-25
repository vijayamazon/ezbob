IF OBJECT_ID('RptAccountingLoanBalance') IS NULL
	EXECUTE('CREATE PROCEDURE RptAccountingLoanBalance AS SELECT 1')
GO

ALTER PROCEDURE RptAccountingLoanBalance
@DateStart DATETIME,
@DateEnd DATETIME
AS
SELECT
	l.Date AS IssueDate,
	c.Id AS ClientID,
	l.Id AS LoanID,
	c.Fullname AS ClientName,
	c.Name AS ClientEmail,
	la.Amount AS IssuedAmount,
	la.Fees AS SetupFee,
	ISNULL(SUM(lc.Amount), 0) AS FeesEarned,
	l.Status AS LoanStatus,
	lm.Name AS LoanTranMethod,
	ISNULL(SUM(ISNULL(t.Amount, 0)), 0) AS TotalRepaid,
	ISNULL(SUM(ISNULL(t.Fees, 0)), 0) AS FeesRepaid,
	ISNULL(SUM(ISNULL(t.Rollover, 0)), 0) AS RolloverRepaid
FROM
	Loan l
	INNER JOIN Customer c ON l.CustomerID = c.Id
	INNER JOIN LoanTransaction la
		ON l.Id = la.LoanId
		AND la.Type = 'PacnetTransaction'
		AND la.Status = 'Done'
	LEFT JOIN LoanTransaction t
		ON l.Id = t.LoanId
		AND t.Type = 'PaypointTransaction'
		AND t.Status = 'Done'
		AND t.PostDate < @DateEnd
	LEFT JOIN LoanCharges lc
		ON l.Id = lc.LoanId
		AND lc.Date < @DateEnd
	LEFT JOIN LoanTransactionMethod lm
		ON t.LoanTransactionMethodId = lm.Id
GROUP BY
	l.Date,
	c.Id,
	l.Id,
	c.Fullname,
	c.Name,
	la.Amount,
	la.Fees,
	l.Status,
	lm.Name
ORDER BY
	l.Id
GO

IF NOT EXISTS (SELECT * FROM ReportScheduler WHERE Type = 'RPT_ACCOUNTING_LOAN_BALANCE')
	INSERT INTO ReportScheduler(Type, Title, StoredProcedure, IsDaily, IsWeekly, IsMonthly, IsMonthToDate, Header, Fields, ToEmail) VALUES (
		'RPT_ACCOUNTING_LOAN_BALANCE', 'Accounting loan balance', 'RptAccountingLoanBalance',
		0, 0, 0, 0, '', '',
		'nimrodk@ezbob.com,alexbo+rpt@ezbob.com'
	)
GO

UPDATE ReportScheduler SET
	Header = 'Issue Date,Client ID,Loan ID,Client Name,Client Email,Loan Status,Issued Amount,Setup Fee,Earned Interest,Earned Fees,Paid Fees,Total Repaid (Cash),Total Repaid (Non-cash),Balance',
	Fields = 'IssueDate,#ClientID,!LoanID,ClientName,ClientEmail,LoanStatus,IssuedAmount,SetupFee,EarnedInterest,EarnedFees,PaidFees,CashPaid,NonCashPaid,Balance'
WHERE
	Type = 'RPT_ACCOUNTING_LOAN_BALANCE'

DELETE ReportArguments FROM
	ReportArguments ra
	INNER JOIN ReportScheduler r ON ra.ReportId = r.Id AND r.Type = 'RPT_ACCOUNTING_LOAN_BALANCE'
GO

INSERT INTO ReportArguments (ReportArgumentNameId, ReportId)
SELECT
	1,
	Id
FROM
	ReportScheduler
WHERE
	Type = 'RPT_ACCOUNTING_LOAN_BALANCE'
GO
