IF NOT EXISTS (SELECT 1 FROM ReportScheduler WHERE Type = 'RPT_TRAFFIC_REPORT') 
BEGIN
	INSERT INTO dbo.ReportScheduler (
		Type, Title, StoredProcedure,
		IsDaily, IsWeekly, IsMonthly,
		Header,
		Fields,
		ToEmail, IsMonthToDate
	)
	VALUES (
		'RPT_TRAFFIC_REPORT', 'Traffic Report', 'NO SP',
		1, 0, 0,
		'Channel,Visits,Visitors,Registrations,Applications,Loans,Loan Amount,Cost,New Customer Cost,ROI',
	    'Channel,Visits,Visitors,Registrations,Applications,Loans,LoanAmount,Cost,NewCustomerCost,ROI',
		'nimrodk@ezbob.com,stasd@ezbob.com', 0
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
		r.Type = 'RPT_TRAFFIC_REPORT'
		AND NOT EXISTS (
			SELECT *
			FROM ReportsUsersMap
			WHERE UserID = u.Id
			AND ReportID = r.ID
		)
		
	DECLARE @ReportId INT  = (SELECT Id FROM ReportScheduler r WHERE r.Type = 'RPT_TRAFFIC_REPORT')
	DECLARE @ReportArgId INT = (SELECT Id FROM ReportArgumentNames WHERE Name = 'DateRange')
	INSERT INTO ReportArguments (ReportArgumentNameId, ReportId) VALUES (@ReportArgId,@ReportId)
END
GO