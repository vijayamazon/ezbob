IF OBJECT_ID('RptUiReportData') IS NULL
	EXECUTE('CREATE PROCEDURE RptUiReportData AS SELECT 1')
GO

ALTER PROCEDURE RptUiReportData
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
		UiControlID,
		UiControlName
	FROM
		UiControls

	-----------------------------------------------------------------------
	--
	-- Helper data: from Customer table.
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
		c.ResidentialStatus,
		c.TypeOfBusiness,
		c.NonLimitedCompanyName,
		c.LimitedCompanyName,
		c.IsOffline
	FROM
		Customer c
		INNER JOIN #RelevantCustomers rc ON c.Id = rc.CustomerID

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
		INNER JOIN #RelevantCustomers rc ON ue.UserID = rc.CustomerID
	WHERE
		@DateStart <= ue.EventTime AND ue.EventTime < @DateEnd
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
