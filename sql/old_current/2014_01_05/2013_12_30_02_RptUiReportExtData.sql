IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('UiControlGroupRelevance') AND name = 'Position')
	ALTER TABLE UiControlGroupRelevance ADD Position INT NULL
GO

-------------------------------------------------------------------------------

IF NOT EXISTS (SELECT * FROM UiControlGroups WHERE UiControlGroupID = 4)
BEGIN
	INSERT INTO UiControlGroups(UiControlGroupID, UiControlGroupName) VALUES (4, 'Entire Wizard')

	INSERT INTO UiControlGroupRelevance(UiControlGroupID, UiControlID)
	SELECT 4, UiControlID FROM UiControlGroupRelevance WHERE UiControlGroupID IN (1, 2, 3)
END
GO

-------------------------------------------------------------------------------

IF OBJECT_ID('RptUiReportExtData') IS NULL
	EXECUTE('CREATE PROCEDURE RptUiReportExtData AS SELECT 1')
GO

ALTER PROCEDURE RptUiReportExtData
@DateStart DATETIME,
@DateEnd DATETIME
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
		w.WizardStepTypeName
	FROM
		Customer c
		INNER JOIN #RelevantCustomers rc ON c.Id = rc.CustomerID
		INNER JOIN WizardStepTypes w ON c.WizardStep = w.WizardStepTypeID

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
