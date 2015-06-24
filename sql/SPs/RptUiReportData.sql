IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptUiReportData]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptUiReportData]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptUiReportData] 
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
	-- Helper data: UI Actions.
	--
	-----------------------------------------------------------------------

	SELECT
		'UiActions' AS TableName,
		UiActionID,
		UiActionName
	FROM
		UiActions

	-----------------------------------------------------------------------
	--
	-- Helper data: UI Controls.
	--
	-----------------------------------------------------------------------

	SELECT
		'UiControls' AS TableName,
		c.UiControlID,
		c.UiControlName,
		r.UiControlGroupID
	FROM
		UiControls c
		INNER JOIN UiControlGroupRelevance r ON c.UiControlID = r.UiControlID
	ORDER BY
		UiControlID

	-----------------------------------------------------------------------
	--
	-- Helper data: from CustomerAddress table.
	--
	-----------------------------------------------------------------------

	SELECT
		'Address' AS TableName,
		a.CustomerId AS CustomerID,
		a.addressType AS AddressTypeID,
		COUNT(DISTINCT a.addressId) AS AddressCount
	FROM
		CustomerAddress a
		INNER JOIN #RelevantCustomers rc ON a.CustomerId = rc.CustomerID
	WHERE
		a.addressType IN (1, 2, 3, 5) -- current, previous, limited, nonlimited
	GROUP BY
		a.CustomerId,
		a.addressType

	-----------------------------------------------------------------------
	--
	-- Helper data: from Director table.
	--
	-----------------------------------------------------------------------

	SELECT
		'Director' AS TableName,
		d.CustomerId AS CustomerID,
		COUNT(DISTINCT d.Id) AS DirectorCount
	FROM
		Director d
		INNER JOIN #RelevantCustomers rc ON d.CustomerId = rc.CustomerID
	GROUP BY
		d.CustomerId

	-----------------------------------------------------------------------
	--
	-- Helper data: from MP_CustomerMarketPlace table.
	--
	-----------------------------------------------------------------------

	SELECT
		'LinkedAccounts' AS TableName,
		m.CustomerId AS CustomerID,
		COUNT(DISTINCT m.Id) AS AccountCount
	FROM
		MP_CustomerMarketPlace m
		INNER JOIN #RelevantCustomers rc ON m.CustomerId = rc.CustomerID
	GROUP BY
		m.CustomerId

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
		c.Gender,
		c.DateOfBirth,
		c.MaritalStatus,
		c.MobilePhone,
		c.DaytimePhone,
		c.TimeAtAddress,
		cps.Description,
		c.TypeOfBusiness,
		co.CompanyName,
		c.IsOffline,
		c.WizardStep,
		w.TheLastOne,
		w.WizardStepTypeName,
		Origin = zl.Name
	FROM
		Customer c
		INNER JOIN CustomerPropertyStatuses cps ON cps.Id = c.PropertyStatusId
		INNER JOIN CustomerOrigin zl ON c.OriginID = zl.CustomerOriginID
		LEFT JOIN Company co ON co.Id = c.CompanyId
		INNER JOIN #RelevantCustomers rc ON c.Id = rc.CustomerID
		INNER JOIN WizardStepTypes w ON c.WizardStep = w.WizardStepTypeID

	-----------------------------------------------------------------------
	--
	-- UI events list.
	--
	-----------------------------------------------------------------------

	SELECT
		'UiEvents' AS TableName,
		ue.UiControlID,
		ue.UiActionID,
		ue.EventTime,
		ue.UserID,
		ue.ControlHtmlID,
		ue.EventArguments
	FROM
		UiEvents ue
		INNER JOIN UiControlGroupRelevance r
			ON ue.UiControlID = r.UiControlID
			AND r.UiControlGroupID IN (1, 2, 3)
		INNER JOIN #RelevantCustomers rc ON ue.UserID = rc.CustomerID
	ORDER BY
		ue.UserID,
		ue.EventTime

	-----------------------------------------------------------------------
	--
	-- Cleanup.
	--
	-----------------------------------------------------------------------

	DROP TABLE #RelevantCustomers
END
GO
