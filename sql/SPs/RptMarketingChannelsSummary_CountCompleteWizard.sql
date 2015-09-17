IF OBJECT_ID('RptMarketingChannelsSummary_CountCompleteWizard') IS NULL
	EXECUTE('CREATE PROCEDURE RptMarketingChannelsSummary_CountCompleteWizard AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptMarketingChannelsSummary_CountCompleteWizard
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	-----------------------------------------------------------

	CREATE TABLE #Cr (
		CustomerID INT,
		CrDate DATETIME
	)

	-----------------------------------------------------------
	--
	-- Customers who completed wizard in any possible way:
	-- they have their very first cash request created on
	-- wizard completion date.
	--
	-----------------------------------------------------------

	INSERT INTO #Cr (CustomerID, CrDate)
	SELECT
		c.Id,
		MIN(r.CreationDate)
	FROM
		Customer c
		INNER JOIN CashRequests r ON c.Id = r.IdCustomer
	WHERE
		c.IsTest = 0
		AND
		r.CreationDate IS NOT NULL
	GROUP BY
		c.Id
	
	-----------------------------------------------------------
	--
	-- Customers who completed wizard during specified period of time.
	--
	-----------------------------------------------------------
	;WITH
	crl_id AS (
		SELECT
			CustomerId,
			MAX(Id) AS Id
		FROM
			CustomerRequestedLoan
		GROUP BY
			CustomerId
	),
	crl AS (
		SELECT
			c.CustomerId,
			c.Amount AS Amount
		FROM
			crl_id d
			INNER JOIN CustomerRequestedLoan c ON d.Id = c.Id
	)
	SELECT
		'CompleteApplication' AS RowType,
		CASE WHEN c.BrokerID IS NULL THEN c.ReferenceSource ELSE 'brk' END AS ReferenceSource,
		c.GoogleCookie,
		c.BrokerID,
		COUNT(*) AS Counter,
		SUM(ISNULL(crl.Amount, 0)) AS Amount
	FROM
		Customer c
		INNER JOIN #Cr r ON c.Id = r.CustomerID
		LEFT JOIN crl ON c.Id = crl.CustomerId
	WHERE
		@DateStart <= r.CrDate AND r.CrDate < @DateEnd
	GROUP BY
		CASE WHEN c.BrokerID IS NULL THEN c.ReferenceSource ELSE 'brk' END,
		c.GoogleCookie,
		c.BrokerID

	DROP TABLE #Cr
END
GO
