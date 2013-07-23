IF OBJECT_ID('udfCustomerMarketPlaces') IS NOT NULL
	DROP FUNCTION udfCustomerMarketPlaces
GO

CREATE FUNCTION udfCustomerMarketPlaces(@CustomerID INT)
RETURNS NVARCHAR(4000)
AS
BEGIN
	DECLARE @out NVARCHAR(4000)

	SET @out = ''

	SELECT
		@out = @out +
			(CASE @out WHEN '' THEN '' ELSE ', ' END) +
			(CASE COUNT(DISTINCT m.Id) WHEN 1 THEN '' ELSE CONVERT(NVARCHAR, COUNT(DISTINCT m.Id)) + ' ' END) +
			t.Name
	FROM
		MP_CustomerMarketPlace m
		INNER JOIN MP_MarketplaceType t ON m.MarketPlaceId = t.Id
	WHERE
		m.CustomerId = @CustomerID
	GROUP BY
		t.Name
	ORDER BY
		t.Name

	RETURN @out
END
GO

IF OBJECT_ID('RptSourceRef') IS NOT NULL
	DROP PROCEDURE RptSourceRef
GO

CREATE PROCEDURE RptSourceRef
@DateStart DATETIME,
@DateEnd   DATETIME
AS
BEGIN
	SET NOCOUNT ON
	
	CREATE TABLE #out (
		TypeID INT NOT NULL,
		CustomerID INT NOT NULL,
		CashRequestID INT NULL,
		LoanID INT NULL,
		Status NVARCHAR(3),
		LoanCount INT NULL,
		MarketPlaces NVARCHAR(4000) NOT NULL DEFAULT ''
	)

	------------------------------------------------------------------------------
	--
	-- Registered
	--
	------------------------------------------------------------------------------

	INSERT INTO #out (TypeID, CustomerID, Status, LoanCount)
	SELECT
		1,
		C.Id,
		'New',
		0
	FROM
		Customer C
	WHERE
		C.IsTest = 0
		AND
		@DateStart <= C.GreetingMailSentDate AND C.GreetingMailSentDate < @DateEnd

	------------------------------------------------------------------------------
	--
	-- Applied
	--
	------------------------------------------------------------------------------

	INSERT INTO #out (TypeID, CustomerId, CashRequestID)
	SELECT
		2,
		R.IdCustomer,
		R.Id
	FROM 
		CashRequests R
		INNER JOIN Customer C ON C.Id = R.IdCustomer AND C.IsTest = 0
	WHERE
		@DateStart <= R.CreationDate AND R.CreationDate < @DateEnd

	------------------------------------------------------------------------------
	--
	-- Approved
	--
	------------------------------------------------------------------------------

	INSERT INTO #out (TypeID, CustomerId, CashRequestID)
	SELECT
		3,
		R.IdCustomer,
		R.Id
	FROM 
		CashRequests R
		INNER JOIN Customer C ON C.Id = R.IdCustomer AND C.IsTest = 0
	WHERE
		@DateStart <= R.UnderwriterDecisionDate AND R.UnderwriterDecisionDate < @DateEnd
		AND
		R.UnderwriterDecision = 'Approved'

	------------------------------------------------------------------------------
	--
	-- Issued
	--
	------------------------------------------------------------------------------

	INSERT INTO #out (TypeID, CustomerId, LoanID)
	SELECT
		4,
		L.CustomerId,
		L.Id
	FROM
		Loan L
		INNER JOIN Customer C ON L.CustomerId = C.Id AND C.IsTest = 0
	WHERE 
		@DateStart <= L.[Date] AND L.[Date] < @DateEnd

	------------------------------------------------------------------------------
	--
	-- Set cash request ID for loans
	--
	------------------------------------------------------------------------------

	UPDATE #out SET
		CashRequestID = (
			SELECT
				MAX(R.Id)
			FROM
				CashRequests R
				INNER JOIN Loan L
					ON R.IdCustomer = L.CustomerId
					AND R.UnderwriterDecisionDate <= L.Date
			WHERE
				#out.CustomerID = R.IdCustomer
		)
	WHERE
		LoanID IS NOT NULL

	------------------------------------------------------------------------------
	--
	-- Set status (old/new)
	--
	------------------------------------------------------------------------------

	UPDATE #out SET
		Status = (CASE
			WHEN EXISTS (
				SELECT
					Id
				FROM
					CashRequests R
				WHERE
					R.IdCustomer = #out.CustomerID
					AND
					R.Id < #out.CashRequestID
			) THEN 'Old'
			ELSE 'New'
		END)
	WHERE
		CashRequestID IS NOT NULL

	------------------------------------------------------------------------------
	--
	-- Set loan count
	--
	------------------------------------------------------------------------------

	UPDATE #out SET
		LoanCount = ISNULL((
			SELECT COUNT (*)
			FROM Loan L
			WHERE L.CustomerId = #out.CustomerID
		), 0)
	WHERE
		CashRequestID IS NOT NULL

	------------------------------------------------------------------------------
	--
	-- Set marketplaces
	--
	------------------------------------------------------------------------------

	UPDATE #out SET
		MarketPlaces = dbo.udfCustomerMarketPlaces(CustomerID)

	------------------------------------------------------------------------------
	--
	-- Output
	--
	------------------------------------------------------------------------------

	SELECT
		(CASE o.TypeID
			WHEN 1 THEN '1: Registered'
			WHEN 2 THEN '2: Applied'
			WHEN 3 THEN '3: Approved'
			WHEN 4 THEN '4: Issued'
		END) AS Type,
		o.CustomerID,
		c.GreetingMailSentDate,
		c.ReferenceSource,
		c.WizardStep,
		r.CreationDate,
		r.UnderwriterDecision,
		r.ManagerApprovedSum,
		l.LoanAmount,
		o.Status,
		o.LoanCount,
		o.MarketPlaces,
		r.InterestRate,
		l.Date AS LoanDate,
		c.FullName,
		c.Name AS Email
	FROM
		#out o
		INNER JOIN Customer c ON o.CustomerID = c.Id
		LEFT JOIN CashRequests r ON o.CashRequestID = r.Id
		LEFT JOIN Loan l ON o.LoanID = l.Id
	ORDER BY
		o.TypeID

	DROP TABLE #out

	SET NOCOUNT OFF
END
GO

UPDATE ReportScheduler SET
	Header='Type,Customer ID,Sign Up Date,Reference Source,Wizard Step,Creation Date,Underwriter Decision,Manager Approved Sum,Loan Amount,Customer Status,Loan Count,Market Places,Interest Rate,Loan Date,Full Name,Email',
	Fields='Type,!CustomerID,GreetingMailSentDate,ReferenceSource,WizardStep,CreationDate,UnderwriterDecision,ManagerApprovedSum,LoanAmount,Status,LoanCount,MarketPlaces,%InterestRate,LoanDate,FullName,Email'
WHERE
	Type='RPT_SOURCE_REF'
GO
