IF OBJECT_ID('RptAlibabaFunnel') IS NULL
	EXECUTE('CREATE PROCEDURE RptAlibabaFunnel AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptAlibabaFunnel
@BatchName NVARCHAR(255)
AS
BEGIN
	SET NOCOUNT ON;

	CREATE TABLE #Funnel (
		DatumID INT NOT NULL,
		DoDropoff BIT NOT NULL,
		Caption NVARCHAR(255) NOT NULL,
		Counter INT NOT NULL
	)

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	IF @BatchName IS NULL
	BEGIN

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

		INSERT INTO #Funnel(DatumID, DoDropoff, Caption, Counter)
		VALUES (10, 1, 'Landing page unique visitors', ISNULL((
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

		INSERT INTO #Funnel(DatumID, DoDropoff, Caption, Counter)
		VALUES (20, 1, 'Start EZBOB appliation', ISNULL((SELECT COUNT(*) FROM #ws), 0))

	------------------------------------------------------------------------------

		DROP TABLE #ws

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	END -- if @BatchName is null

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #Funnel(DatumID, DoDropoff, Caption, Counter)
	VALUES (30, 1, 'Signed up', ISNULL((
		SELECT
			COUNT(*)
		FROM
			Customer c
			INNER JOIN CampaignSourceRef csr
				ON c.Id = csr.CustomerId
				AND (@BatchName IS NULL OR csr.FName = @BatchName)
		WHERE
			c.AlibabaId IS NOT NULL
			AND
			c.IsTest = 0
	), 0))

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #Funnel(DatumID, DoDropoff, Caption, Counter)
	VALUES (40, 1, 'Filled personal details', ISNULL((
		SELECT
			COUNT(*)
		FROM
			Customer c
			INNER JOIN CampaignSourceRef csr
				ON c.Id = csr.CustomerId
				AND (@BatchName IS NULL OR csr.FName = @BatchName)
		WHERE
			c.AlibabaId IS NOT NULL
			AND
			c.IsTest = 0
			AND
			c.DateOfBirth IS NOT NULL
	), 0))

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #Funnel(DatumID, DoDropoff, Caption, Counter)
	VALUES (50, 1, 'Filled company details', ISNULL((
		SELECT
			COUNT(*)
		FROM
			Customer c
			INNER JOIN Company co
				ON c.CompanyId = co.Id
				AND co.CompanyName IS NOT NULL
			INNER JOIN CampaignSourceRef csr
				ON c.Id = csr.CustomerId
				AND (@BatchName IS NULL OR csr.FName = @BatchName)
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
		INNER JOIN CampaignSourceRef csr
			ON c.Id = csr.CustomerId
			AND (@BatchName IS NULL OR csr.FName = @BatchName)
	WHERE
		c.AlibabaId IS NOT NULL
		AND
		c.IsTest = 0
	GROUP BY
		c.Id

	------------------------------------------------------------------------------

	DECLARE @CompleteWizardCount INT = ISNULL((SELECT COUNT(*) FROM #cr), 0)

	------------------------------------------------------------------------------

	DECLARE @OfflineCompleteWizardCount INT = ISNULL((
		SELECT
			COUNT(DISTINCT cr.CustomerID)
		FROM
			#cr cr
			INNER JOIN CustomerRelations ros
				ON cr.CustomerID = ros.CustomerId
				AND cr.CrDate >= ros.Timestamp
	), 0)

	------------------------------------------------------------------------------

	DROP TABLE #cr

	------------------------------------------------------------------------------

	INSERT INTO #Funnel(DatumID, DoDropoff, Caption, Counter)
	VALUES (60, 0, 'Linked accounts and finished wizard', @CompleteWizardCount)

	------------------------------------------------------------------------------

	INSERT INTO #Funnel(DatumID, DoDropoff, Caption, Counter)
	VALUES (63, 0, '* Linked accounts automatically', @CompleteWizardCount - @OfflineCompleteWizardCount)

	------------------------------------------------------------------------------

	INSERT INTO #Funnel(DatumID, DoDropoff, Caption, Counter)
	VALUES (67, 0, '* Complete application via offline channels', @OfflineCompleteWizardCount)

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #Funnel(DatumID, DoDropoff, Caption, Counter)
	VALUES (70, 0, 'Total completed applications', @CompleteWizardCount)

	------------------------------------------------------------------------------

	DECLARE @ApprovedCount INT = ISNULL((
		SELECT
			COUNT(DISTINCT cr.IdCustomer)
		FROM
			CashRequests cr
			INNER JOIN Customer c
				ON cr.IdCustomer = c.Id
				AND c.IsTest = 0
				AND c.AlibabaId IS NOT NULL
			INNER JOIN CampaignSourceRef csr
				ON c.Id = csr.CustomerId
				AND (@BatchName IS NULL OR csr.FName = @BatchName)
		WHERE
			cr.UnderwriterDecision = 'Approved'
	), 0)

	------------------------------------------------------------------------------

	DECLARE @RejectedCount INT = ISNULL((
		SELECT
			COUNT(DISTINCT cr.IdCustomer)
		FROM
			CashRequests cr
			INNER JOIN Customer c
				ON cr.IdCustomer = c.Id
				AND c.IsTest = 0
				AND c.AlibabaId IS NOT NULL
			INNER JOIN CampaignSourceRef csr
				ON c.Id = csr.CustomerId
				AND (@BatchName IS NULL OR csr.FName = @BatchName)
		WHERE
			cr.UnderwriterDecision = 'Rejected'
	), 0)

	------------------------------------------------------------------------------

	INSERT INTO #Funnel(DatumID, DoDropoff, Caption, Counter)
	VALUES (72, 0, '* Approved', @ApprovedCount)

	------------------------------------------------------------------------------

	INSERT INTO #Funnel(DatumID, DoDropoff, Caption, Counter)
	VALUES (74, 0, '* Rejected', @RejectedCount)

	------------------------------------------------------------------------------

	INSERT INTO #Funnel(DatumID, DoDropoff, Caption, Counter)
	VALUES (76, 0, '* Waiting for additional information', @CompleteWizardCount - @ApprovedCount - @RejectedCount)

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #Funnel(DatumID, DoDropoff, Caption, Counter)
	VALUES (80, 0, 'Started taking loan', ISNULL((
		SELECT
			COUNT(DISTINCT l.CustomerId)
		FROM
			Loan l
			INNER JOIN Customer c
				ON l.CustomerId = c.Id
				AND c.IsTest = 0
				AND c.AlibabaId IS NOT NULL
			INNER JOIN CampaignSourceRef csr
				ON c.Id = csr.CustomerId
				AND (@BatchName IS NULL OR csr.FName = @BatchName)
	), 0))

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SELECT
		RowType = 'Funnel',
		DoDropoff,
		Caption,
		Counter
	FROM
		#Funnel
	ORDER BY
		DatumID

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	DROP TABLE #Funnel

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
		INNER JOIN CampaignSourceRef csr
			ON c.Id = csr.CustomerId
			AND (@BatchName IS NULL OR csr.FName = @BatchName)
	WHERE
		cr.UnderwriterDecision = 'Rejected'
	GROUP BY
		rr.Reason
END
GO
