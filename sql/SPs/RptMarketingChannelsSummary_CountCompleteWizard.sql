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
		INNER JOIN CustomerRequestedLoan crl ON c.Id = crl.CustomerId
	WHERE
		@DateStart <= r.CrDate AND r.CrDate < @DateEnd
	GROUP BY
		CASE WHEN c.BrokerID IS NULL THEN c.ReferenceSource ELSE 'brk' END,
		c.GoogleCookie,
		c.BrokerID

	DROP TABLE #Cr
END
GO
