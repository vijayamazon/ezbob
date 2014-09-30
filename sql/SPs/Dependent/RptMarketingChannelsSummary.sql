IF OBJECT_ID('RptMarketingChannelsSummary') IS NULL
	EXECUTE('CREATE PROCEDURE RptMarketingChannelsSummary AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptMarketingChannelsSummary
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	-----------------------------------------------------------
	--
	-- Calculate visitors
	--
	-----------------------------------------------------------

	SELECT
		'Visitors' AS RowType,
		sa.Source,
		ISNULL(SUM(ISNULL(sa.SiteAnalyticsValue, 0)), 0) AS 'Visitors'
	FROM
		SiteAnalytics sa
		INNER JOIN SiteAnalyticsCodes c
			ON sa.SiteAnalyticsCode = c.Id
			AND c.Name = 'SourceUsers'
	WHERE
		@DateStart <= sa.[Date] AND sa.[Date] < @DateEnd
	GROUP BY
		sa.Source

	-----------------------------------------------------------
	--
	-- Calculate start registration
	--
	-----------------------------------------------------------

	SELECT
		'StartRegistration' AS RowType,
		CASE WHEN c.BrokerID IS NULL THEN c.ReferenceSource ELSE 'brk' END AS ReferenceSource,
		c.GoogleCookie,
		c.BrokerID,
		COUNT(*) AS Counter
	FROM
		Customer c
	WHERE
		c.IsTest = 0
		AND
		@DateStart <= c.GreetingMailSentDate AND c.GreetingMailSentDate < @DateEnd
	GROUP BY
		CASE WHEN c.BrokerID IS NULL THEN c.ReferenceSource ELSE 'brk' END,
		c.GoogleCookie,
		c.BrokerID

	-----------------------------------------------------------
	--
	-- Calculate personal
	--
	-----------------------------------------------------------

	EXECUTE RptMarketingChannelsSummary_CountCustomersOnSteps 'Personal', @DateStart, @DateEnd, 'personal-info:continue'

	-----------------------------------------------------------
	--
	-- Calculate company
	--
	-----------------------------------------------------------

	EXECUTE RptMarketingChannelsSummary_CountCustomersOnSteps 'Company', @DateStart, @DateEnd, 'personal-info:company_continue'

	-----------------------------------------------------------
	--
	-- Calculate datasource
	--
	-----------------------------------------------------------

	SELECT
		'DataSource' AS RowType,
		CASE WHEN c.BrokerID IS NULL THEN c.ReferenceSource ELSE 'brk' END AS ReferenceSource,
		c.GoogleCookie,
		c.BrokerID,
		COUNT(DISTINCT c.Id) AS Counter
	FROM
		Customer c
		INNER JOIN MP_CustomerMarketPlace m ON c.Id = m.CustomerId
	WHERE
		c.IsTest = 0
		AND
		@DateStart <= m.Created AND m.Created < @DateEnd
	GROUP BY
		CASE WHEN c.BrokerID IS NULL THEN c.ReferenceSource ELSE 'brk' END,
		c.GoogleCookie,
		c.BrokerID

	-----------------------------------------------------------
	--
	-- Calculate complete application
	--
	-----------------------------------------------------------

	EXECUTE RptMarketingChannelsSummary_CountCompleteWizard @DateStart, @DateEnd

	-----------------------------------------------------------
	--
	-- Calculate requested amount
	--
	-----------------------------------------------------------

	SELECT
		'RequestedAmount' AS RowType,
		CASE WHEN c.BrokerID IS NULL THEN c.ReferenceSource ELSE 'brk' END AS ReferenceSource,
		c.GoogleCookie,
		c.BrokerID,
		SUM(ISNULL(crl.Amount, 0)) AS Amount
	FROM
		Customer c
		INNER JOIN CustomerRequestedLoan crl ON c.Id = crl.CustomerId
	WHERE
		c.IsTest = 0
		AND
		@DateStart <= c.GreetingMailSentDate AND c.GreetingMailSentDate < @DateEnd
	GROUP BY
		CASE WHEN c.BrokerID IS NULL THEN c.ReferenceSource ELSE 'brk' END,
		c.GoogleCookie,
		c.BrokerID

	-----------------------------------------------------------
	--
	-- Calculate approved/rejected/pending count.
	--
	-----------------------------------------------------------

	SELECT
		'ApprovedRejected' AS RowType,
		SUM(CASE r.UnderwriterDecision
			WHEN 'Approved' THEN 1
			ELSE 0
		END) AS NumOfApproved,
		SUM(CASE r.UnderwriterDecision
			WHEN 'Rejected' THEN 1
			ELSE 0
		END) AS NumOfRejected,
		SUM(CASE r.UnderwriterDecision
			WHEN 'Approved' THEN 0
			WHEN 'Rejected' THEN 0
			ELSE 1
		END) AS NumOfPending,
		CASE WHEN c.BrokerID IS NULL THEN c.ReferenceSource ELSE 'brk' END AS ReferenceSource,
		c.GoogleCookie,
		c.BrokerID
	FROM
		Customer c
		INNER JOIN CashRequests r ON c.Id = r.IdCustomer
	WHERE
		@DateStart <= r.UnderwriterDecisionDate AND r.UnderwriterDecisionDate < @DateEnd
		AND
		c.IsTest = 0
	GROUP BY
		CASE WHEN c.BrokerID IS NULL THEN c.ReferenceSource ELSE 'brk' END,
		c.GoogleCookie,
		c.BrokerID

	-----------------------------------------------------------
	--
	-- Calculate approved didn't take.
	--
	-----------------------------------------------------------

	SELECT
		'ApprovedDidntTake' AS RowType,
		CASE WHEN c.BrokerID IS NULL THEN c.ReferenceSource ELSE 'brk' END AS ReferenceSource,
		c.GoogleCookie,
		c.BrokerID,
		COUNT(*) AS Counter
	FROM
		Customer c
		INNER JOIN CashRequests r ON c.Id = r.IdCustomer
		LEFT JOIN Loan l
			ON c.Id = l.CustomerId
			AND @DateStart <= l.[Date]
	WHERE
		@DateStart <= r.UnderwriterDecisionDate AND r.UnderwriterDecisionDate < @DateEnd
		AND
		c.IsTest = 0
		AND
		r.UnderwriterDecision = 'Approved'
		AND
		l.Id IS NULL
	GROUP BY
		CASE WHEN c.BrokerID IS NULL THEN c.ReferenceSource ELSE 'brk' END,
		c.GoogleCookie,
		c.BrokerID

	-----------------------------------------------------------
	--
	-- Calculate approved amount.
	--
	-----------------------------------------------------------

	;WITH FirstCR (CustomerID, CashRequestID) AS (
		SELECT
			r.IdCustomer AS CustomerID,
			MIN(r.Id) AS CashRequestID
		FROM
			CashRequests r
			INNER JOIN Customer c ON r.IdCustomer = c.Id AND c.IsTest = 0
		WHERE
			r.UnderwriterDecision = 'Approved'
		GROUP BY
			r.IdCustomer
	)
	SELECT
		'ApprovedAmount' AS RowType,
		CASE WHEN c.BrokerID IS NULL THEN c.ReferenceSource ELSE 'brk' END AS ReferenceSource,
		c.GoogleCookie,
		c.BrokerID,
		SUM(ISNULL(r.ManagerApprovedSum, 0)) AS Amount
	FROM
		Customer c
		INNER JOIN CashRequests r ON c.Id = r.IdCustomer
		INNER JOIN FirstCR f ON r.Id = f.CashRequestID
	WHERE
		c.IsTest = 0
		AND
		@DateStart <= r.UnderwriterDecisionDate AND r.UnderwriterDecisionDate < @DateEnd
		AND
		r.UnderwriterDecision = 'Approved'
	GROUP BY
		CASE WHEN c.BrokerID IS NULL THEN c.ReferenceSource ELSE 'brk' END,
		c.GoogleCookie,
		c.BrokerID

	-----------------------------------------------------------
	--
	-- Calculate loans given.
	--
	-----------------------------------------------------------

	SELECT
		'LoansGiven' AS RowType,
		CASE WHEN c.BrokerID IS NULL THEN c.ReferenceSource ELSE 'brk' END AS ReferenceSource,
		c.GoogleCookie,
		c.BrokerID,
		SUM(ISNULL(l.LoanAmount, 0)) AS Amount
	FROM
		Customer c
		INNER JOIN Loan l ON c.Id = l.CustomerId
	WHERE
		c.IsTest = 0
		AND
		@DateStart <= l.[Date] AND l.[Date] < @DateEnd
		AND
		l.Position = 0
	GROUP BY
		CASE WHEN c.BrokerID IS NULL THEN c.ReferenceSource ELSE 'brk' END,
		c.GoogleCookie,
		c.BrokerID
END
GO
