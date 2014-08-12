IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptSourceRef]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptSourceRef]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptSourceRef] 
	(@DateStart DATETIME,
@DateEnd   DATETIME)
AS
BEGIN
	SET NOCOUNT ON	

	SET @DateStart = CONVERT(DATE, @DateStart)
	SET @DateEnd = CONVERT(DATE, @DateEnd)

	CREATE TABLE #out (
		TypeID INT NOT NULL,
		CustomerID INT NOT NULL,
		CashRequestID INT NULL,
		LoanID INT NULL,
		Status NVARCHAR(3),
		LoanCount INT NULL,
		MarketPlaces NVARCHAR(4000) NOT NULL DEFAULT ''
	)

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

	UPDATE #out SET
		LoanCount = ISNULL((
			SELECT COUNT (*)
			FROM Loan L
			WHERE L.CustomerId = #out.CustomerID
		), 0)
	WHERE
		CashRequestID IS NOT NULL

	UPDATE #out SET
		MarketPlaces = dbo.udfCustomerMarketPlaces(CustomerID)

	SELECT
		(CASE o.TypeID
			WHEN 1 THEN '1: Registered'
			WHEN 2 THEN '2: Applied'
			WHEN 3 THEN '3: Approved'
			WHEN 4 THEN '4: Issued'
		END) AS Type,
		o.CustomerID,
		c.GreetingMailSentDate,
		(CASE WHEN c.BrokerID IS NULL THEN c.ReferenceSource ELSE 'Broker' END) AS ReferenceSource,
		c.GoogleCookie,
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
