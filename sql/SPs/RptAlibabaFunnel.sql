IF OBJECT_ID('RptAlibabaFunnel') IS NULL
	EXECUTE('CREATE PROCEDURE RptAlibabaFunnel AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptAlibabaFunnel
AS
BEGIN
	SET NOCOUNT ON;

	CREATE TABLE #EclFunnel (
		DatumID INT NOT NULL,
		IsEcl BIT NOT NULL,
		IsEzbob BIT NOT NULL,
		Caption NVARCHAR(255) NOT NULL,
		Counter INT NOT NULL
	)

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #EclFunnel(DatumID, IsEcl, IsEzbob, Caption, Counter)
	VALUES (10, 1, 0, 'Landing page unique visitors', ISNULL((
		SELECT
			SUM(a.SiteAnalyticsValue)
		FROM
			SiteAnalytics a
			INNER JOIN SiteAnalyticsCodes ac
				ON a.SiteAnalyticsCode = ac.Id
				AND ac.Name = 'LandingPageNewUsers'
		WHERE
			a.Source LIKE '/alibaba%'
	), 0))

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SELECT
		e.SessionCookie,
		e.UserID,
		COUNT(*) AS Counter
	INTO
		#ws
	FROM
		UiEvents e
		INNER JOIN UiActions a
			ON e.UiActionID = a.UiActionID
			AND a.UiActionName = 'pageload'
	WHERE
		e.ControlHtmlID = 'Customer/Wizard'
		AND
		e.EventArguments LIKE 'alibaba_id:%'
	GROUP BY
		e.SessionCookie,
		e.UserID

	------------------------------------------------------------------------------

	INSERT INTO #EclFunnel(DatumID, IsEcl, IsEzbob, Caption, Counter)
	VALUES (20, 1, 1, 'Start EZBOB appliation', ISNULL((SELECT COUNT(*) FROM #ws), 0))

	------------------------------------------------------------------------------

	DROP TABLE #ws

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #EclFunnel(DatumID, IsEcl, IsEzbob, Caption, Counter)
	VALUES (30, 0, 1, 'Signed up', ISNULL((
		SELECT
			COUNT(*)
		FROM
			Customer c
		WHERE
			c.AlibabaId IS NOT NULL
			AND
			c.IsTest = 0
	), 0))

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #EclFunnel(DatumID, IsEcl, IsEzbob, Caption, Counter)
	VALUES (40, 0, 1, 'Filled personal details', ISNULL((
		SELECT
			COUNT(*)
		FROM
			Customer c
		WHERE
			c.AlibabaId IS NOT NULL
			AND
			c.IsTest = 0
			AND
			c.DateOfBirth IS NOT NULL
	), 0))

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #EclFunnel(DatumID, IsEcl, IsEzbob, Caption, Counter)
	VALUES (50, 0, 1, 'Filled company details', ISNULL((
		SELECT
			COUNT(*)
		FROM
			Customer c
			INNER JOIN Company co ON c.CompanyId = co.Id
				AND co.CompanyName IS NOT NULL
		WHERE
			c.AlibabaId IS NOT NULL
			AND
			c.IsTest = 0
	), 0))

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SELECT
		c.Id AS CustomerID,
		MIN(r.CreationDate) AS CrDate
	INTO
		#cr
	FROM
		Customer c
		INNER JOIN CashRequests r ON c.Id = r.IdCustomer
	WHERE
		c.AlibabaId IS NOT NULL
		AND
		c.IsTest = 0
	GROUP BY
		c.Id

	------------------------------------------------------------------------------

	INSERT INTO #EclFunnel(DatumID, IsEcl, IsEzbob, Caption, Counter)
	VALUES (60, 1, 1, 'Linked accounts and finished wizard', ISNULL((SELECT COUNT(*) FROM #cr), 0))

	------------------------------------------------------------------------------

	INSERT INTO #EclFunnel(DatumID, IsEcl, IsEzbob, Caption, Counter)
	VALUES (70, 1, 0, 'Complete application via offline channels', ISNULL((
		SELECT
			COUNT(DISTINCT cr.CustomerID)
		FROM
			#cr cr
			INNER JOIN CustomerRelations ros
				ON cr.CustomerID = ros.CustomerId
				AND cr.CrDate >= ros.Timestamp
	), 0))

	------------------------------------------------------------------------------

	DROP TABLE #cr

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #EclFunnel(DatumID, IsEcl, IsEzbob, Caption, Counter)
	VALUES (80, 1, 0, 'How many approved', ISNULL((
		SELECT
			COUNT(DISTINCT cr.IdCustomer)
		FROM
			CashRequests cr
			INNER JOIN Customer c
				ON cr.IdCustomer = c.Id
				AND c.IsTest = 0
				AND c.AlibabaId IS NOT NULL
		WHERE
			cr.UnderwriterDecision = 'Approved'
	), 0))

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #EclFunnel(DatumID, IsEcl, IsEzbob, Caption, Counter)
	VALUES (90, 1, 0, 'Started taking loan', ISNULL((
		SELECT
			COUNT(DISTINCT l.CustomerId)
		FROM
			Loan l
			INNER JOIN Customer c
				ON l.CustomerId = c.Id
				AND c.IsTest = 0
				AND c.AlibabaId IS NOT NULL
	), 0))

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SELECT
		RowType = 'EclFunnel',
		Caption,
		Counter
	FROM
		#EclFunnel
	WHERE
		IsEcl = 1
	ORDER BY
		DatumID

	------------------------------------------------------------------------------

	SELECT
		RowType = 'EzbobFunnel',
		Caption,
		Counter
	FROM
		#EclFunnel
	WHERE
		IsEzbob = 1
	ORDER BY
		DatumID

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	DROP TABLE #EclFunnel

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SELECT
		RowType = 'RejectReason',
		Caption = rr.Reason,
		Counter = COUNT(*)
	FROM
		CashRequests cr
		INNER JOIN DecisionHistory dh ON cr.Id = dh.CashRequestId
		INNER JOIN DecisionHistoryRejectReason dhrr ON dh.Id = dhrr.DecisionHistoryId
		INNER JOIN RejectReason rr ON dhrr.RejectReasonId = rr.Id
		INNER JOIN Customer c
			ON cr.IdCustomer = c.Id
			AND c.IsTest = 0
			AND c.AlibabaId IS NOT NULL
	WHERE
		cr.UnderwriterDecision = 'Rejected'
	GROUP BY
		rr.Reason
END
GO
