IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptExecutive]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptExecutive]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptExecutive] 
	(@DateStart DATETIME, @DateEnd DATETIME)
AS
BEGIN
	SELECT
		@DateStart = CONVERT(DATE, @DateStart),
		@DateEnd = CONVERT(DATE, @DateEnd)

	------------------------------------------------------------------------------

	DECLARE @TotalGivenLoanCountClose INT 
	DECLARE @TotalGivenLoanCountLive INT
	
	DECLARE @TotalGivenLoanValueClose NUMERIC(18, 2)
	DECLARE @TotalRepaidPrincipalClose NUMERIC(18, 2)

	DECLARE @PACNET NVARCHAR(32) = 'PacnetTransaction'
	DECLARE @PAYPOINT NVARCHAR(32) = 'PaypointTransaction'
	DECLARE @DONE NVARCHAR(4) = 'Done'
	DECLARE @Indent NVARCHAR(48) = '&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;'

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	CREATE TABLE #out (
		SortOrder INT IDENTITY(1, 1) NOT NULL,
		Caption NVARCHAR(300) NOT NULL,
		Number SQL_VARIANT,    -- INT
		Amount SQL_VARIANT,    -- DECIMAL(18, 2)
		Principal SQL_VARIANT, -- DECIMAL(18, 2)
		Interest SQL_VARIANT,  -- DECIMAL(18, 2)
		Fees SQL_VARIANT,      -- DECIMAL(18, 2)
		Css NVARCHAR(256) NULL
	)

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #out(Caption, Number, Amount, Principal, Interest, Fees, Css)
		VALUES ('Visitors', 'Number', '', '', '', '', 'total')

	------------------------------------------------------------------------------

	SELECT
		0 AS SortOrder,
		Name, 
		s.Description AS Caption
	INTO
		#t
	FROM
		SiteAnalyticsCodes s
	WHERE
		1 = 0

	------------------------------------------------------------------------------

	
	INSERT INTO #t (SortOrder, Name, Caption) VALUES (1, 'WorldWideVisits', 'Total visits')
	INSERT INTO #t (SortOrder, Name, Caption) VALUES (2, 'WorldWideVisitors', 'Total visitors')
	INSERT INTO #t (SortOrder, Name, Caption) VALUES (3, 'UKVisits', 'UK visits')
	INSERT INTO #t (SortOrder, Name, Caption) VALUES (4, 'UKVisitors', 'UK visitors')
	INSERT INTO #t (SortOrder, Name, Caption) VALUES (5, 'NewVisitors', 'New visitors')
	
	INSERT INTO #t (SortOrder, Name, Caption) VALUES (6, 'PageLogon', 'Login page visitors')
	INSERT INTO #t (SortOrder, Name, Caption) VALUES (7, 'PageDashboard', 'Customer dashboard page visitors')
	INSERT INTO #t (SortOrder, Name, Caption) VALUES (8, 'PageGetCash', 'Get cash page visitors')
	INSERT INTO #t (SortOrder, Name, Caption) VALUES (9, 'PagePacnet', 'Pacnet page visitors')
	

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number)
	SELECT
		#t.Caption,
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
		#t.Caption,
		#t.SortOrder
	ORDER BY
		#t.SortOrder

	DROP TABLE #t

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #out(Caption, Number, Amount, Principal, Interest, Fees, Css)
		VALUES ('Funnel', 'Number', '', '', '', '', 'total')

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
		'Person details',
		ISNULL(COUNT(*), 0)
	FROM
		Customer c
	WHERE
		c.IsTest = 0
		AND
		c.WizardStep IN (4,5,6)
		AND
		@DateStart <= c.GreetingMailSentDate AND c.GreetingMailSentDate < @DateEnd
		
	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number)
	SELECT
		'Company details',
		ISNULL(COUNT(*), 0)
	FROM
		Customer c
	WHERE
		c.IsTest = 0
		AND
		c.WizardStep IN (4,6)
		AND
		@DateStart <= c.GreetingMailSentDate AND c.GreetingMailSentDate < @DateEnd
		
	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number)
	SELECT
		'Accounts',
		ISNULL(COUNT(*), 0)
	FROM
		Customer c
	WHERE
		c.IsTest = 0
		AND
		c.WizardStep = 4
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
		c.CreditResult IS NOT NULL
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
		'Loans',
		ISNULL(COUNT(DISTINCT l.CustomerId), 0)
	FROM
		Customer c
		INNER JOIN Loan l ON c.Id = l.CustomerId
	WHERE
		c.IsTest = 0
		AND
		@DateStart <= c.GreetingMailSentDate AND c.GreetingMailSentDate < @DateEnd
		AND 
		@DateStart <= l.[Date] AND l.[Date] < @DateEnd
		
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

	INSERT INTO #out(Caption, Number, Amount, Principal, Interest, Fees, Css)
		VALUES ('Daily Approvals', 'Number', 'Amount', 'Principal', 'Interest', 'Fees', 'total')

	------------------------------------------------------------------------------

	SELECT
		r.IdCustomer AS CustomerID,
		ISNULL(COUNT(DISTINCT r.Id), 0) AS RequestCount,
		ISNULL(MAX(r.ManagerApprovedSum), 0) AS MaxApprovedSum,
		CONVERT(INT, 0) AS OldRequestCount
	INTO
		#cr
	FROM
		CashRequests r
		INNER JOIN Customer c ON r.IdCustomer = c.Id AND c.IsTest = 0
	WHERE
		@DateStart <= r.UnderwriterDecisionDate AND r.UnderwriterDecisionDate < @DateEnd
		AND
		r.UnderwriterDecision = 'Approved'
	GROUP BY
		r.IdCustomer

	------------------------------------------------------------------------------

	UPDATE #cr SET
		OldRequestCount = ISNULL((
			SELECT COUNT(DISTINCT Id)
			FROM CashRequests old
			WHERE old.IdCustomer = #cr.CustomerID
			AND old.UnderwriterDecisionDate < @DateStart
		), 0)

	------------------------------------------------------------------------------

	INSERT INTO #out(Caption, Number, Amount)
	SELECT
		'Total',
		ISNULL(COUNT(DISTINCT CustomerID), 0),
		ISNULL(SUM(MaxApprovedSum), 0)
	FROM
		#cr

	------------------------------------------------------------------------------

	INSERT INTO #out(Caption, Number, Amount)
	SELECT
		'New',
		ISNULL(COUNT(DISTINCT CustomerID), 0),
		ISNULL(SUM(MaxApprovedSum), 0)
	FROM
		#cr
	WHERE
		OldRequestCount = 0

	------------------------------------------------------------------------------

	INSERT INTO #out(Caption, Number, Amount)
	SELECT
		'Old',
		ISNULL(COUNT(DISTINCT CustomerID), 0),
		ISNULL(SUM(MaxApprovedSum), 0)
	FROM
		#cr
	WHERE
		OldRequestCount != 0

	------------------------------------------------------------------------------

	DROP TABLE #cr

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #out(Caption, Number, Amount, Principal, Interest, Fees, Css)
		VALUES ('Issued Loans', 'Number', 'Amount', 'Principal', 'Interest', 'Fees', 'total')

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
		t.Amount,
		0,
		0
	FROM
		Customer c
		INNER JOIN Loan l ON c.Id = l.CustomerId AND c.IsTest = 0
		INNER JOIN LoanTransaction t ON l.Id = t.LoanId
	WHERE
		t.Status = @DONE AND t.Type = @PACNET
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
		@Indent + 'New loans',
		ISNULL(COUNT(*), 0),
		ISNULL(SUM(ISNULL(LoanAmount, 0)), 0)
	FROM
		#l
	WHERE
		PreviousLoansCount = 0

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number, Amount)
	SELECT
		@Indent + 'Existing loans',
		ISNULL(COUNT(*), 0),
		ISNULL(SUM(ISNULL(LoanAmount, 0)), 0)
	FROM
		#l
	WHERE
		PreviousLoansCount != 0

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number, Amount)
	SELECT
		@Indent + @Indent + 'Existing fully paid',
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
		@Indent + @Indent + 'Existing open loans',
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

	CREATE TABLE #known_tran_time_status (
		TransactionID INT,
		Status INT
	)

	------------------------------------------------------------------------------

	INSERT INTO #known_tran_time_status
	SELECT DISTINCT
		TransactionID,
		CASE StatusBefore
			WHEN 'Late' THEN 1
			WHEN 'Paid' THEN 1
			WHEN 'PaidEarly' THEN 2
			ELSE 3
		END
	FROM
		LoanScheduleTransaction
	UNION
	SELECT DISTINCT
		TransactionID,
		CASE StatusAfter
			WHEN 'Late' THEN 1
			WHEN 'Paid' THEN 1
			WHEN 'PaidEarly' THEN 2
			ELSE 3
		END
	FROM
		LoanScheduleTransaction

	------------------------------------------------------------------------------

	SELECT
		TransactionID,
		MIN(Status) AS Status
	INTO
		#grouped_tran_time_status
	FROM
		#known_tran_time_status
	GROUP BY
		TransactionID

	------------------------------------------------------------------------------

	SELECT
		t.Id AS TransactionID,
		ISNULL(g.Status, 3) AS Status
	INTO
		#tran_time_status
	FROM
		LoanTransaction t
		LEFT JOIN #grouped_tran_time_status g ON t.Id = g.TransactionID

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #out(Caption, Number, Amount, Principal, Interest, Fees, Css)
		VALUES ('Repayments', 'Number', 'Amount', 'Principal', 'Interest', 'Fees', 'total')

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number, Amount, Principal, Interest, Fees)
	SELECT
		'Total',
		ISNULL(COUNT(t.Id), 0),
		ISNULL(SUM(t.Amount), 0),
		ISNULL(SUM(t.LoanRepayment), 0),
		ISNULL(SUM(t.Interest), 0),
		ISNULL(SUM(t.Fees), 0)
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanID = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	WHERE
		@DateStart <= t.PostDate AND t.PostDate < @DateEnd
		AND
		t.Type = @PAYPOINT AND t.Status = @DONE

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number, Amount, Principal, Interest, Fees)
	SELECT
		'Early payments',
		ISNULL(COUNT(t.Id), 0),
		ISNULL(SUM(t.Amount), 0),
		ISNULL(SUM(t.LoanRepayment), 0),
		ISNULL(SUM(t.Interest), 0),
		ISNULL(SUM(t.Fees), 0)
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanID = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
		INNER JOIN #tran_time_status s ON t.Id = s.TransactionID AND s.Status = 2
	WHERE
		@DateStart <= t.PostDate AND t.PostDate < @DateEnd
		AND
		t.Type = @PAYPOINT AND t.Status = @DONE

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number, Amount, Principal, Interest, Fees)
	SELECT
		'On time payments',
		ISNULL(COUNT(t.Id), 0),
		ISNULL(SUM(t.Amount), 0),
		ISNULL(SUM(t.LoanRepayment), 0),
		ISNULL(SUM(t.Interest), 0),
		ISNULL(SUM(t.Fees), 0)
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanID = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
		INNER JOIN #tran_time_status s ON t.Id = s.TransactionID AND s.Status = 3
	WHERE
		@DateStart <= t.PostDate AND t.PostDate < @DateEnd
		AND
		t.Type = @PAYPOINT AND t.Status = @DONE

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number, Amount, Principal, Interest, Fees)
	SELECT
		'Late payments',
		ISNULL(COUNT(t.Id), 0),
		ISNULL(SUM(t.Amount), 0),
		ISNULL(SUM(t.LoanRepayment), 0),
		ISNULL(SUM(t.Interest), 0),
		ISNULL(SUM(t.Fees), 0)
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanID = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
		INNER JOIN #tran_time_status s ON t.Id = s.TransactionID AND s.Status = 1
	WHERE
		@DateStart <= t.PostDate AND t.PostDate < @DateEnd
		AND
		t.Type = @PAYPOINT AND t.Status = @DONE

	------------------------------------------------------------------------------

	DROP TABLE #tran_time_status
	DROP TABLE #grouped_tran_time_status
	DROP TABLE #known_tran_time_status

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number, Amount)
	SELECT
		'Loans paid fully',
		ISNULL(COUNT(DISTINCT l.Id), 0),
		ISNULL(SUM(l.LoanAmount), 0)
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

	INSERT INTO #out(Caption, Number, Amount, Principal, Interest, Fees, Css)
		VALUES ('Total Book', 'Number', 'Amount', 'Principal', 'Interest', 'Fees', 'total')

	------------------------------------------------------------------------------

	SELECT
		@TotalGivenLoanCountClose = ISNULL( COUNT(DISTINCT t.LoanId), 0 ),
		@TotalGivenLoanValueClose = ISNULL( SUM(ISNULL(t.Amount, 0)), 0 )
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	WHERE
		t.Type = @PACNET AND t.Status = @DONE
		AND
		t.PostDate < @DateEnd
	
	------------------------------------------------------------------------------
	
	SELECT 
		@TotalGivenLoanCountLive = ISNULL(COUNT(*), 0) 
	FROM 
		Customer c JOIN Loan l ON c.Id = l.CustomerId 
	WHERE 
		c.IsTest = 0 
		AND  
		l.Status<>'PaidOff'
		AND l.[Date] < @DateEnd

	------------------------------------------------------------------------------

	SELECT
		@TotalRepaidPrincipalClose = ISNULL( SUM(ISNULL(t.LoanRepayment, 0)), 0 )
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	WHERE
		t.Type = @PAYPOINT AND t.Status = @DONE
		AND
		t.PostDate < @DateEnd

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number, Amount)
	SELECT
		'Total issued loans',
		@TotalGivenLoanCountClose,
		@TotalGivenLoanValueClose

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number, Amount, Principal, Interest, Fees)
	SELECT
		'Total repayments',
		ISNULL(COUNT(DISTINCT t.Id), 0),
		ISNULL(SUM(t.Amount), 0),
		ISNULL(SUM(t.LoanRepayment), 0),
		ISNULL(SUM(t.Interest), 0),
		ISNULL(SUM(t.Fees), 0)
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	WHERE
		t.Type = @PAYPOINT AND t.Status = @DONE
		AND
		t.PostDate < @DateEnd

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number, Amount)
	SELECT
		'Outstanding balance',
		@TotalGivenLoanCountLive,
		@TotalGivenLoanValueClose - @TotalRepaidPrincipalClose

	------------------------------------------------------------------------------
	
	INSERT INTO #out(Caption, Number, Amount, Principal, Interest, Fees, Css)
	  	VALUES ('Loans by customer status without CCI mark', 'Customers', 'Loan Amount', 'Balance', '', '', 'total')

	------------------------------------------------------------------------------
	
	INSERT INTO #out (Caption, Number, Amount, Principal)
	SELECT
	 	S.Name,
		count(1) Customers,
		sum(L.LoanAmount) LoanAmount,
		sum(L.Balance) Balance
	FROM Customer C,
		 CustomerStatuses S,
		 Loan L
	WHERE 
		C.IsTest = 0 AND 
		L.CustomerId = c.Id AND
		L.Status != 'PaidOff' and
		C.CollectionStatus = S.Id AND
		C.CciMark = 0
	GROUP BY S.Name
	ORDER BY sum(L.LoanAmount) DESC	
	
	------------------------------------------------------------------------------
	
	INSERT INTO #out(Caption, Number, Amount, Principal, Interest, Fees, Css)
		VALUES ('Loans by customer status with CCI mark', 'Customers', 'Loan Amount', 'Balance', '', '', 'total')

	------------------------------------------------------------------------------
	
	INSERT INTO #out (Caption, Number, Amount, Principal)
	SELECT 
	    S.Name,
		count(1) Customers,
		sum(L.LoanAmount) LoanAmount,
		sum(L.Balance) Balance
	FROM Customer C,
		 CustomerStatuses S,
		 Loan L
	WHERE 
		C.IsTest = 0 AND 
		L.CustomerId = c.Id AND
		L.Status != 'PaidOff' and
		C.CollectionStatus = S.Id AND
		C.CciMark = 1
	GROUP BY S.Name
	ORDER BY sum(L.LoanAmount) DESC	
	
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
