IF NOT EXISTS (SELECT 1 FROM ReportScheduler WHERE Type = 'RPT_STEP_ONE_CUSTOMERS') 
BEGIN
	INSERT INTO dbo.ReportScheduler (
		Type, Title, StoredProcedure,
		IsDaily, IsWeekly, IsMonthly,
		Header,
		Fields,
		ToEmail, IsMonthToDate
	)
	VALUES (
		'RPT_STEP_ONE_CUSTOMERS', 'Step One Customers', 'RptStepOneCustomers',
		0, 0, 0,
		'Customer Id,eMail,First name,Phone',
	    '#cId,eMail,FirstName,Phone',
		'nimrodk@ezbob.com', 0
	)

	INSERT INTO ReportsUsersMap (UserID, ReportID)
	SELECT
		u.Id,
		r.Id
	FROM
		ReportUsers u,
		ReportScheduler r
	WHERE
		u.UserName IN ('alexbo', 'stasd', 'nimrodk', 'eilaya')
		AND
		r.Type = 'RPT_STEP_ONE_CUSTOMERS'
		AND NOT EXISTS (
			SELECT *
			FROM ReportsUsersMap
			WHERE UserID = u.Id
			AND ReportID = r.ID
		)
		
	DECLARE @ReportId INT  = (SELECT Id FROM ReportScheduler r WHERE r.Type = 'RPT_STEP_ONE_CUSTOMERS')
	DECLARE @ReportArgId INT = (SELECT Id FROM ReportArgumentNames WHERE Name = 'DateRange')
	INSERT INTO ReportArguments (ReportArgumentNameId, ReportId) VALUES (@ReportArgId,@ReportId)
END
GO	
	