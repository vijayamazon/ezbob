IF OBJECT_ID('RptExecutive') IS NOT NULL
	DROP PROCEDURE RptExecutive
GO

CREATE PROCEDURE RptExecutive
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	CREATE TABLE #out (
		SortOrder INT IDENTITY(1, 1) NOT NULL,
		Caption NVARCHAR(300) NOT NULL,
		Number INT NULL,
		Amount DECIMAL(18, 2) NULL,
		Principal DECIMAL(18, 2) NULL,
		Interest DECIMAL(18, 2) NULL,
		Fees DECIMAL(18, 2) NULL,
		Css NVARCHAR(256) NULL
	)

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #out(Caption, Css) VALUES ('Visitors', 'total')

	------------------------------------------------------------------------------
	-- Create a temp table with right collation
	------------------------------------------------------------------------------

	SELECT
		0 AS SortOrder,
		Name
	INTO
		#t
	FROM
		SiteAnalyticsCodes
	WHERE
		1 = 0

	------------------------------------------------------------------------------
	
	INSERT INTO #t (SortOrder, Name) VALUES (1, 'UKVisitors')
	INSERT INTO #t (SortOrder, Name) VALUES (2, 'ReturningVisitors')
	INSERT INTO #t (SortOrder, Name) VALUES (3, 'NewVisitors')
	INSERT INTO #t (SortOrder, Name) VALUES (4, 'PageDashboard')
	INSERT INTO #t (SortOrder, Name) VALUES (5, 'PageLogon')
	INSERT INTO #t (SortOrder, Name) VALUES (6, 'PagePacnet')
	INSERT INTO #t (SortOrder, Name) VALUES (7, 'PageGetCash')

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number)
	SELECT
		c.Description,
		ISNULL(SUM(ISNULL(a.SiteAnalyticsValue, 0)), 0)
	FROM
		#t
		LEFT JOIN SiteAnalyticsCodes c ON #t.Name = c.Name
		LEFT JOIN SiteAnalytics a ON c.Id = a.SiteAnalyticsCode
	WHERE
		c.Name = #t.Name
		AND
		@DateStart <= a.Date AND a.Date < @DateEnd
	GROUP BY
		c.Description,
		#t.SortOrder
	ORDER BY
		#t.SortOrder

	------------------------------------------------------------------------------

	DROP TABLE #t

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #out(Caption, Css) VALUES ('Funnel', 'total')

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number)
	SELECT
		'Registrations',
		ISNULL(COUNT(*), 0)
	FROM
		Customer c
	WHERE
		c.IsTest = 0
		AND
		@DateStart <= c.GreetingMailSentDate AND c.GreetingMailSentDate < @DateEnd

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number)
	SELECT
		'Entered data source',
		ISNULL(COUNT(DISTINCT m.CustomerId), 0)
	FROM
		MP_CustomerMarketPlace m
		INNER JOIN Customer c ON m.CustomerId = c.Id
	WHERE
		c.IsTest = 0
		AND
		@DateStart <= c.GreetingMailSentDate AND c.GreetingMailSentDate < @DateEnd

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number)
	SELECT
		'Finished',
		ISNULL(COUNT(*), 0)
	FROM
		Customer c
	WHERE
		c.WizardStep >= 3
		AND
		c.IsTest = 0
		AND
		@DateStart <= c.GreetingMailSentDate AND c.GreetingMailSentDate < @DateEnd

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number)
	SELECT
		'Approved',
		ISNULL(COUNT(DISTINCT r.IdCustomer), 0)
	FROM
		Customer c
		INNER JOIN CashRequests r ON c.Id = r.IdCustomer
	WHERE
		r.UnderwriterDecision = 'Approved'
		AND
		c.IsTest = 0
		AND
		@DateStart <= c.GreetingMailSentDate AND c.GreetingMailSentDate < @DateEnd

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number)
	SELECT
		'Rejected',
		ISNULL(COUNT(DISTINCT r.IdCustomer), 0)
	FROM
		Customer c
		INNER JOIN CashRequests r ON c.Id = r.IdCustomer
	WHERE
		r.UnderwriterDecision = 'Rejected'
		AND
		NOT EXISTS (
			SELECT acr.Id
			FROM CashRequests acr
			WHERE acr.IdCustomer = r.IdCustomer
			AND acr.UnderwriterDecision = 'Approved'
		)
		AND
		c.IsTest = 0
		AND
		@DateStart <= c.GreetingMailSentDate AND c.GreetingMailSentDate < @DateEnd

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number)
	SELECT
		'Pending',
		ISNULL(COUNT(DISTINCT r.IdCustomer), 0)
	FROM
		Customer c
		INNER JOIN CashRequests r ON c.Id = r.IdCustomer
	WHERE
		NOT EXISTS (
			SELECT acr.Id
			FROM CashRequests acr
			WHERE acr.IdCustomer = r.IdCustomer
			AND acr.UnderwriterDecision IN ('Approved', 'Rejected')
		)
		AND
		c.IsTest = 0
		AND
		@DateStart <= c.GreetingMailSentDate AND c.GreetingMailSentDate < @DateEnd

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #out(Caption, Css) VALUES ('Issued Loans', 'total')

	------------------------------------------------------------------------------

	CREATE TABLE #l (
		LoanID INT,
		CustomerID INT,
		LoanAmount DECIMAL(18, 2),
		PreviousLoansCount INT,
		PaidOffLoansCount INT
	)

	------------------------------------------------------------------------------

	INSERT INTO #l (LoanID, CustomerID, LoanAmount, PreviousLoansCount, PaidOffLoansCount)
	SELECT
		l.Id,
		l.CustomerId,
		l.LoanAmount,
		0,
		0
	FROM
		Customer c
		INNER JOIN Loan l ON c.Id = l.CustomerId
	WHERE
		c.IsTest = 0
		AND
		@DateStart <= l.Date AND l.Date < @DateEnd

	------------------------------------------------------------------------------

	UPDATE #l SET
		PreviousLoansCount = (
			SELECT ISNULL(COUNT(*), 0)
			FROM Loan l
			WHERE #l.CustomerID = l.CustomerId
			AND #l.LoanID > l.Id
		)

	------------------------------------------------------------------------------

	UPDATE #l SET
		PaidOffLoansCount = (
			SELECT ISNULL(COUNT(*), 0)
			FROM Loan l
			WHERE #l.CustomerID = l.CustomerId
			AND #l.LoanID != l.Id
			AND l.Status = 'PaidOff'
		)

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number, Amount)
	SELECT
		'Total',
		ISNULL(COUNT(*), 0),
		ISNULL(SUM(ISNULL(LoanAmount, 0)), 0)
	FROM
		#l

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number, Amount)
	SELECT
		'New loans',
		ISNULL(COUNT(*), 0),
		ISNULL(SUM(ISNULL(LoanAmount, 0)), 0)
	FROM
		#l
	WHERE
		PreviousLoansCount = 0

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number, Amount)
	SELECT
		'Existing loans',
		ISNULL(COUNT(*), 0),
		ISNULL(SUM(ISNULL(LoanAmount, 0)), 0)
	FROM
		#l
	WHERE
		PreviousLoansCount != 0

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number, Amount)
	SELECT
		'Existing fully paid',
		ISNULL(COUNT(*), 0),
		ISNULL(SUM(ISNULL(LoanAmount, 0)), 0)
	FROM
		#l
	WHERE
		PreviousLoansCount != 0
		AND
		PaidOffLoansCount = PreviousLoansCount

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number, Amount)
	SELECT
		'Existing open loans',
		ISNULL(COUNT(*), 0),
		ISNULL(SUM(ISNULL(LoanAmount, 0)), 0)
	FROM
		#l
	WHERE
		PreviousLoansCount != 0
		AND
		PaidOffLoansCount != PreviousLoansCount

	------------------------------------------------------------------------------

	DROP TABLE #l

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #out(Caption, Css) VALUES ('Repayments', 'total')

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number, Amount, Principal, Interest, Fees)
	SELECT
		'Total',
		ISNULL(COUNT(lst.TransactionID), 0),
		ISNULL(SUM(ABS(lst.PrincipalDelta) + ABS(lst.InterestDelta) + ABS(lst.FeesDelta)), 0),
		ISNULL(SUM(ABS(lst.PrincipalDelta)), 0),
		ISNULL(SUM(ABS(lst.InterestDelta)), 0),
		ISNULL(SUM(ABS(lst.FeesDelta)), 0)
	FROM
		LoanScheduleTransaction lst
		INNER JOIN Loan l ON lst.LoanID = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id
	WHERE
		c.IsTest = 0
		AND
		@DateStart <= lst.Date AND lst.Date < @DateEnd

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number, Amount, Principal, Interest, Fees)
	SELECT
		'Early payments',
		ISNULL(COUNT(lst.TransactionID), 0),
		ISNULL(SUM(ABS(lst.PrincipalDelta) + ABS(lst.InterestDelta) + ABS(lst.FeesDelta)), 0),
		ISNULL(SUM(ABS(lst.PrincipalDelta)), 0),
		ISNULL(SUM(ABS(lst.InterestDelta)), 0),
		ISNULL(SUM(ABS(lst.FeesDelta)), 0)
	FROM
		LoanScheduleTransaction lst
		INNER JOIN Loan l ON lst.LoanID = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id
	WHERE
		lst.StatusAfter IN ('PaidEarly', 'StillToPay')
		AND
		c.IsTest = 0
		AND
		@DateStart <= lst.Date AND lst.Date < @DateEnd

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number, Amount, Principal, Interest, Fees)
	SELECT
		'On time payments',
		ISNULL(COUNT(lst.TransactionID), 0),
		ISNULL(SUM(ABS(lst.PrincipalDelta) + ABS(lst.InterestDelta) + ABS(lst.FeesDelta)), 0),
		ISNULL(SUM(ABS(lst.PrincipalDelta)), 0),
		ISNULL(SUM(ABS(lst.InterestDelta)), 0),
		ISNULL(SUM(ABS(lst.FeesDelta)), 0)
	FROM
		LoanScheduleTransaction lst
		INNER JOIN Loan l ON lst.LoanID = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id
	WHERE
		lst.StatusAfter IN ('PaidOnTime')
		AND
		c.IsTest = 0
		AND
		@DateStart <= lst.Date AND lst.Date < @DateEnd

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number, Amount, Principal, Interest, Fees)
	SELECT
		'Late payments',
		ISNULL(COUNT(lst.TransactionID), 0),
		ISNULL(SUM(ABS(lst.PrincipalDelta) + ABS(lst.InterestDelta) + ABS(lst.FeesDelta)), 0),
		ISNULL(SUM(ABS(lst.PrincipalDelta)), 0),
		ISNULL(SUM(ABS(lst.InterestDelta)), 0),
		ISNULL(SUM(ABS(lst.FeesDelta)), 0)
	FROM
		LoanScheduleTransaction lst
		INNER JOIN Loan l ON lst.LoanID = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id
	WHERE
		lst.StatusAfter IN ('Paid', 'Late')
		AND
		c.IsTest = 0
		AND
		@DateStart <= lst.Date AND lst.Date < @DateEnd

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number)
	SELECT
		'Loans paid fully',
		ISNULL(COUNT(DISTINCT l.Id), 0)
	FROM
		Loan l
		INNER JOIN Customer c ON l.CustomerId = c.Id
	WHERE
		c.IsTest = 0
		AND
		@DateStart <= l.DateClosed AND l.DateClosed < @DateEnd

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #out(Caption, Css) VALUES ('Total book', 'total')

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number, Amount)
	SELECT
		'Total issued loans',
		ISNULL(COUNT(DISTINCT l.Id), 0),
		ISNULL(SUM(ISNULL(l.LoanAmount, 0)), 0)
	FROM
		Customer c
		INNER JOIN Loan l ON c.Id = l.CustomerId
	WHERE
		c.IsTest = 0
		AND
		l.Date < @DateEnd

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number, Amount, Principal, Interest, Fees)
	SELECT
		'Total repayments',
		ISNULL(COUNT(DISTINCT t.Id), 0),
		ISNULL(SUM(t.LoanRepayment + t.Interest + t.Fees), 0),
		ISNULL(SUM(t.LoanRepayment), 0),
		ISNULL(SUM(t.Interest), 0),
		ISNULL(SUM(t.Fees), 0)
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id
	WHERE
		c.IsTest = 0
		AND
		t.Type = 'PaypointTransaction'
		AND
		t.Status = 'Done'
		AND
		t.PostDate < @DateEnd

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number, Amount)
	SELECT
		'Outstanding balance',
		ISNULL(COUNT(DISTINCT l.Id), 0),
		ISNULL(SUM(ISNULL(l.Balance, 0)), 0)
	FROM
		Customer c
		INNER JOIN Loan l ON c.Id = l.CustomerId
	WHERE
		l.Status != 'PaidOff'
		AND
		c.IsTest = 0
		AND
		l.Date < @DateEnd

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SELECT
		Caption,
		Number,
		Amount,
		Principal,
		Interest,
		Fees,
		Css
	FROM
		#out
	ORDER BY
		SortOrder

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	DROP TABLE #out
END
GO

IF NOT EXISTS (SELECT * FROM ReportScheduler WHERE Type = 'RPT_EXECUTIVE')
	INSERT INTO ReportScheduler (Type,Title,StoredProcedure,IsDaily,IsWeekly,IsMonthly,Header,Fields,ToEmail,IsMonthToDate)
		VALUES(
			'RPT_EXECUTIVE', 'Executive', 'RptExecutive', 1, 0, 0,
			'Caption,Number,Amount,Principal,Interest,Fees,Css',
			'Caption,Number,Amount,Principal,Interest,Fees,{Css',
			'nimrodk@ezbob.com,alexbo+rpt@ezbob.com',
			0
		)
ELSE
	UPDATE ReportScheduler SET
		Title = 'Executive',
		StoredProcedure = 'RptExecutive',
		IsDaily = 1,
		IsWeekly = 0,
		IsMonthly = 0,
		Header = 'Caption,Number,Amount,Principal,Interest,Fees,Css',
		Fields = 'Caption,Number,Amount,Principal,Interest,Fees,{Css',
		IsMonthToDate = 0
	WHERE
		Type = 'RPT_EXECUTIVE'
GO
