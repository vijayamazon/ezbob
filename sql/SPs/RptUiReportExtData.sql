IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptUiReportExtData]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptUiReportExtData]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptUiReportExtData] 
	(@DateStart DATETIME,
@DateEnd DATETIME)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		@DateStart = CONVERT(DATE, ISNULL(@DateStart, 'Jul 1 1976')),
		@DateEnd = CONVERT(DATE, ISNULL(@DateEnd, DATEADD(day, 1, GETDATE())))

	-----------------------------------------------------------------------
	--
	-- Select only customers that are either offline or online and have at
	-- least one marketplace.
	--
	-----------------------------------------------------------------------

	SELECT
		ic.Id AS CustomerID
	INTO
		#RelevantCustomers
	FROM
		Customer ic
		INNER JOIN UiEvents ue
			ON ic.Id = ue.UserID
			AND @DateStart <= ue.EventTime AND ue.EventTime < @DateEnd
		LEFT JOIN MP_CustomerMarketPlace imp ON ic.Id = imp.CustomerId
	WHERE
		ic.IsTest = 0
	GROUP BY
		ic.Id,
		ic.IsOffline
	HAVING
		ic.IsOffline = 1
		OR
		COUNT(DISTINCT imp.Id) > 0

	-----------------------------------------------------------------------
	--
	-- Helper data: UI Controls.
	--
	-----------------------------------------------------------------------

	SELECT
		'UiControls' AS TableName,
		c.UiControlID,
		c.UiControlName,
		r.Position
	FROM
		UiControls c
		INNER JOIN UiControlGroupRelevance r
			ON c.UiControlID = r.UiControlID
			AND r.UiControlGroupID = 4
	ORDER BY
		c.UiControlID,
		r.Position

	-----------------------------------------------------------------------
	--
	-- Helper data: from Customer table.
	--
	-- MUST be the last helper data block.
	--
	-----------------------------------------------------------------------

	SELECT
		'Customer' AS TableName,
		c.Id AS CustomerID,
		c.FirstName,
		c.Surname,
		c.TypeOfBusiness,
		c.IsOffline,
		w.TheLastOne,
		w.WizardStepTypeName,
		Origin = zl.Name
	FROM
		Customer c
		INNER JOIN #RelevantCustomers rc ON c.Id = rc.CustomerID
		INNER JOIN WizardStepTypes w ON c.WizardStep = w.WizardStepTypeID
		INNER JOIN CustomerOrigin zl ON c.OriginID = zl.CustomerOriginID

	-----------------------------------------------------------------------
	--
	-- UI events list.
	--
	-----------------------------------------------------------------------

	SELECT DISTINCT
		'UiEvents' AS TableName,
		ue.UiControlID,
		ue.UserID
	FROM
		UiEvents ue
		INNER JOIN UiControlGroupRelevance r
			ON ue.UiControlID = r.UiControlID
			AND r.UiControlGroupID IN (4)
		INNER JOIN #RelevantCustomers rc ON ue.UserID = rc.CustomerID
	ORDER BY
		ue.UserID,
		ue.UiControlID

	-----------------------------------------------------------------------
	--
	-- Cleanup.
	--
	-----------------------------------------------------------------------

	DROP TABLE #RelevantCustomers
END
GO
