IF OBJECT_ID('RptLoansGiven') IS NOT NULL
	DROP PROCEDURE RptLoansGiven
GO

CREATE PROCEDURE RptLoansGiven
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	CREATE TABLE #t (
		LoanID INT NOT NULL,
		Date DATETIME NULL,
		ClientID INT NOT NULL,
		ClientEmail NVARCHAR(128) NOT NULL,
		ClientName NVARCHAR(752) NOT NULL,
		LoanTypeName NVARCHAR(250) NOT NULL,
		SetupFee DECIMAL(18, 4) NOT NULL,
		LoanAmount NUMERIC(18, 0) NOT NULL,
		Period INT NOT NULL,
		PlannedInterest NUMERIC(38, 2) NOT NULL,
		PlannedRepaid NUMERIC(38, 2) NOT NULL,
		RowLevel NVARCHAR(5) NOT NULL
	)

	INSERT INTO #t
	SELECT
		l.Id AS LoanID,
		l.Date,
		c.Id AS ClientID,
		c.Name AS ClientEmail,
		c.FirstName + ' ' + c.MiddleInitial + ' ' + c.Surname AS ClientName,
		lt.Name AS LoanTypeName,
		l.SetupFee,
		l.LoanAmount,
		ISNULL(COUNT(*), 0) AS Period,
		ISNULL(SUM(s.Interest), 0) AS PlannedInterest,
		ISNULL(SUM(s.AmountDue), 0) AS PlannedRepaid,
		''
	FROM
		Loan l
		INNER JOIN Customer c ON l.CustomerId = c.Id
		INNER JOIN LoanType lt ON l.LoanTypeId = lt.Id
		INNER JOIN LoanSchedule s ON l.Id = s.LoanId
	WHERE
		l.Date BETWEEN @DateStart AND @DateEnd
		AND
		c.IsTest = 0
	GROUP BY
		l.Id,
		l.Date,
		c.Id,
		c.Name,
		c.FirstName + ' ' + c.MiddleInitial + ' ' + c.Surname,
		lt.Name,
		l.SetupFee,
		l.LoanAmount

	INSERT INTO #t
	SELECT
		COUNT(DISTINCT LoanID),
		NULL,
		COUNT(DISTINCT ClientID),
		'' AS ClientEmail,
		'Total' AS ClientName,
		'' AS LoanTypeName,
		ISNULL(SUM(SetupFee), 0),
		ISNULL(SUM(LoanAmount), 0),
		ISNULL(AVG(Period), 0),
		ISNULL(SUM(PlannedInterest), 0),
		ISNULL(SUM(PlannedRepaid), 0),
		'total'
	FROM
		#t
	WHERE
		RowLevel = ''

	SELECT
		LoanID,
		Date,
		ClientID,
		ClientEmail,
		ClientName,
		LoanTypeName,
		SetupFee,
		LoanAmount,
		Period,
		PlannedInterest,
		PlannedRepaid,
		RowLevel
	FROM
		#t
	ORDER BY
		RowLevel DESC,
		Date

	DROP TABLE #t
END
GO

IF NOT EXISTS (SELECT * FROM ReportScheduler WHERE Type = 'RPT_LOANS_GIVEN')
BEGIN
	INSERT INTO ReportScheduler VALUES('RPT_LOANS_GIVEN', 'Loans Issued', 'RptLoansGiven', 0, 0, 0,
	'Date,Client ID,Loan ID,Client Name,Client Email,Loan Type,Setup Fee,Amount,Period,Planned Interest,Planned Repaid,Level',
	'Date,!ClientID,!LoanID,ClientName,ClientEmail,LoanTypeName,SetupFee,LoanAmount,Period,PlannedInterest,PlannedRepaid,{RowLevel',
	'nimrodk@ezbob.com,alexbo+rpt@ezbob.com', 1)
END
ELSE BEGIN
	UPDATE ReportScheduler SET
		Title = 'Loans Issued',
		StoredProcedure = 'RptLoansGiven',
		IsDaily = 0,
		IsWeekly = 0,
		IsMonthly = 0,
		Header = 'Date,Client ID,Loan ID,Client Name,Client Email,Loan Type,Setup Fee,Amount,Period,Planned Interest,Planned Repaid,Level',
		Fields = 'Date,!ClientID,!LoanID,ClientName,ClientEmail,LoanTypeName,SetupFee,LoanAmount,Period,PlannedInterest,PlannedRepaid,{RowLevel',
		IsMonthToDate = 1
	WHERE
		Type = 'RPT_LOANS_GIVEN'
END
GO

IF OBJECT_ID('RptPaymentsReceived') IS NOT NULL
	DROP PROCEDURE RptPaymentsReceived
GO

CREATE PROCEDURE RptPaymentsReceived
@DateStart DATETIME,
@DateEnd DATETIME
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
		c.FirstName + ' ' + c.MiddleInitial + ' ' + c.Surname AS ClientName,
		ISNULL(t.Amount, 0),
		ISNULL(t.LoanRepayment, 0),
		ISNULL(t.Interest, 0),
		ISNULL(t.Fees, 0),
		ISNULL(t.Rollover, 0),
		CASE PaypointId WHEN '--- manual ---' THEN 'Manual' ELSE 'Paypoint' END AS TransactionType,
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
	WHERE
		t.PostDate BETWEEN @DateStart AND @DateEnd
		AND
		t.Status = 'Done'
		AND
		c.IsTest = 0
		AND
		t.Type = 'PaypointTransaction'

		
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

IF NOT EXISTS (SELECT * FROM ReportScheduler WHERE Type = 'RPT_PAYMENTS_RECEIVED')
BEGIN
	INSERT INTO ReportScheduler VALUES('RPT_PAYMENTS_RECEIVED', 'Payments Received', 'RptPaymentsReceived', 0, 0, 0,
		'Date,Client ID,Loan ID,Client Name,Client Email,Paid Amount,Principal,Interest,Fees,Rollover,Payment Type,Description,Sum Match,Level',
		'PostDate,!ClientID,!LoanID,ClientName,ClientEmail,Amount,LoanRepayment,Interest,Fees,Rollover,TransactionType,Description,{SumMatch,{RowLevel',
		'nimrodk@ezbob.com,alexbo+rpt@ezbob.com', 1)
END
ELSE BEGIN
	UPDATE ReportScheduler SET
		Title = 'Payments Received',
		StoredProcedure = 'RptPaymentsReceived',
		IsDaily = 0,
		IsWeekly = 0,
		IsMonthly = 0,
		Header = 'Date,Loan ID,Client ID,Client Email,Client Name,Paid Amount,Principal,Interest,Fees,Rollover,Payment Type,Description,Sum Match,Level',
		Fields = 'PostDate,!LoanID,!ClientID,ClientEmail,ClientName,Amount,LoanRepayment,Interest,Fees,Rollover,TransactionType,Description,{SumMatch,{RowLevel',
		IsMonthToDate = 1
	WHERE
		Type = 'RPT_PAYMENTS_RECEIVED'
END
GO

