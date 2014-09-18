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

	DECLARE @Steps IntList
	
	-----------------------------------------------------------
	--
	-- Calculate visitors
	--
	-----------------------------------------------------------
	
	SELECT
		'Visitors' AS RowType,
		sa.Source,
		SUM(sa.SiteAnalyticsValue) AS 'Visitors'
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
	-- Load all the wizard steps
	--
	-----------------------------------------------------------
	
	INSERT INTO @Steps(Value)
	SELECT
		WizardStepTypeID
	FROM
		WizardStepTypes
	
	-----------------------------------------------------------
	--
	-- Calculate start registration
	--
	-----------------------------------------------------------
	
	EXECUTE RptMarketingChannelsSummary_CountCustomersOnSteps 'StartRegistration', @DateStart, @DateEnd, @Steps
	
	-----------------------------------------------------------
	
	DELETE FROM @Steps WHERE Value IN (1, 3)
	
	-----------------------------------------------------------
	--
	-- Calculate personal
	--
	-----------------------------------------------------------
	
	EXECUTE RptMarketingChannelsSummary_CountCustomersOnSteps 'Personal', @DateStart, @DateEnd, @Steps
	
	-----------------------------------------------------------
	
	DELETE FROM @Steps WHERE Value IN (5)
	
	-----------------------------------------------------------
	--
	-- Calculate company
	--
	-----------------------------------------------------------
	
	EXECUTE RptMarketingChannelsSummary_CountCustomersOnSteps 'Company', @DateStart, @DateEnd, @Steps
	
	-----------------------------------------------------------
	
	DELETE FROM @Steps WHERE Value IN (6)
	
	-----------------------------------------------------------
	--
	-- Calculate datasource
	--
	-----------------------------------------------------------
	
	EXECUTE RptMarketingChannelsSummary_CountCustomersOnSteps 'DataSource', @DateStart, @DateEnd, @Steps
	
	-----------------------------------------------------------
	
	DELETE FROM @Steps WHERE Value IN (2)
	
	-----------------------------------------------------------
	--
	-- Calculate complete application
	--
	-----------------------------------------------------------
	
	EXECUTE RptMarketingChannelsSummary_CountCustomersOnSteps 'CompleteApplication', @DateStart, @DateEnd, @Steps
	
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
	-- Calculate approved/rejected count.
	--
	-----------------------------------------------------------
	
	SELECT
		'ApprovedRejected' AS RowType,
		SUM(CASE WHEN c.NumApproves > 0 THEN 1 ELSE 0 END) AS NumOfApproved,
		SUM(CASE WHEN c.NumRejects > 0 AND c.NumApproves = 0 THEN 1 ELSE 0 END) AS NumOfRejected,
		CASE WHEN c.BrokerID IS NULL THEN c.ReferenceSource ELSE 'brk' END AS ReferenceSource,
		c.GoogleCookie,
		c.BrokerID
	FROM
		Customer c
	WHERE
		@DateStart <= c.GreetingMailSentDate AND c.GreetingMailSentDate < @DateEnd
		AND
		c.IsTest = 0
	GROUP BY
		CASE WHEN c.BrokerID IS NULL THEN c.ReferenceSource ELSE 'brk' END,
		c.GoogleCookie,
		c.BrokerID
	
	-----------------------------------------------------------
	--
	-- Calculate approved amount.
	--
	-----------------------------------------------------------
	
	SELECT
		'ApprovedAmount' AS RowType,
		CASE WHEN c.BrokerID IS NULL THEN c.ReferenceSource ELSE 'brk' END AS ReferenceSource,
		c.GoogleCookie,
		c.BrokerID,
		SUM(ISNULL(r.ManagerApprovedSum, 0)) AS Amount
	FROM
		Customer c
		INNER JOIN CashRequests r ON c.Id = r.IdCustomer
	WHERE
		@DateStart <= c.GreetingMailSentDate AND c.GreetingMailSentDate < @DateEnd
		AND
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
		@DateStart <= c.GreetingMailSentDate AND c.GreetingMailSentDate < @DateEnd
		AND
		c.IsTest = 0
		AND
		@DateStart <= l.[Date] AND l.[Date] < @DateEnd
	GROUP BY
		CASE WHEN c.BrokerID IS NULL THEN c.ReferenceSource ELSE 'brk' END,
		c.GoogleCookie,
		c.BrokerID
END
GO
