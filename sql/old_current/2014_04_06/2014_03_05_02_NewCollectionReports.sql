IF NOT EXISTS (SELECT 1 FROM ReportScheduler WHERE Type = 'RPT_ARREARS_LETTER') 
BEGIN
	INSERT INTO dbo.ReportScheduler (
		Type, Title, StoredProcedure,
		IsDaily, IsWeekly, IsMonthly,
		Header,
		Fields,
		ToEmail, IsMonthToDate
	)
	VALUES (
		'RPT_ARREARS_LETTER', 'Collection Arrears Letter', 'RptArrearsLetter',
		1, 0, 0,
		'Payment Date,Client Id,Loan Id,Missed Payments,Name,First Name,Surname,Amount Due',
	    'PaymentDate,#ClientId,LoanId,MissedPayments,Name,FirstName,Surname,AmountDue',
		'emma@ezbob.com', 0
	)

	INSERT INTO ReportsUsersMap (UserID, ReportID)
	SELECT
		u.Id,
		r.Id
	FROM
		ReportUsers u,
		ReportScheduler r
	WHERE
		u.UserName IN ('alexbo', 'stasd', 'nimrodk', 'eilaya', 'emanuellea')
		AND
		r.Type = 'RPT_ARREARS_LETTER'
		AND NOT EXISTS (
			SELECT *
			FROM ReportsUsersMap
			WHERE UserID = u.Id
			AND ReportID = r.ID
		)
		
	DECLARE @ReportId INT  = (SELECT Id FROM ReportScheduler r WHERE r.Type = 'RPT_ARREARS_LETTER')
	DECLARE @ReportArgId INT = (SELECT Id FROM ReportArgumentNames WHERE Name = 'DateRange')
	INSERT INTO ReportArguments (ReportArgumentNameId, ReportId) VALUES (@ReportArgId,@ReportId)
END
GO

IF NOT EXISTS (SELECT 1 FROM ReportScheduler WHERE Type = 'RPT_DEFAULT_LETTER') 
BEGIN
	INSERT INTO dbo.ReportScheduler (
		Type, Title, StoredProcedure,
		IsDaily, IsWeekly, IsMonthly,
		Header,
		Fields,
		ToEmail, IsMonthToDate
	)
	VALUES (
		'RPT_DEFAULT_LETTER', 'Collection Default Letter', 'RptDefaultLetter',
		1, 0, 0,
		'Id,Name,First Name,Surname,Amount Due',
	    '#Id,Name,FirstName,Surname,AmountDue',
		'emma@ezbob.com', 0
	)

	INSERT INTO ReportsUsersMap (UserID, ReportID)
	SELECT
		u.Id,
		r.Id
	FROM
		ReportUsers u,
		ReportScheduler r
	WHERE
		u.UserName IN ('alexbo', 'stasd', 'nimrodk', 'eilaya', 'emanuellea')
		AND
		r.Type = 'RPT_DEFAULT_LETTER'
		AND NOT EXISTS (
			SELECT *
			FROM ReportsUsersMap
			WHERE UserID = u.Id
			AND ReportID = r.ID
		)
		
	DECLARE @ReportId INT  = (SELECT Id FROM ReportScheduler r WHERE r.Type = 'RPT_DEFAULT_LETTER')
	DECLARE @ReportArgId INT = (SELECT Id FROM ReportArgumentNames WHERE Name = 'DateRange')
	INSERT INTO ReportArguments (ReportArgumentNameId, ReportId) VALUES (@ReportArgId,@ReportId)
END
GO

IF NOT EXISTS (SELECT 1 FROM ReportScheduler WHERE Type = 'RPT_STATUS_CHANGED') 
BEGIN
	INSERT INTO dbo.ReportScheduler (
		Type, Title, StoredProcedure,
		IsDaily, IsWeekly, IsMonthly,
		Header,
		Fields,
		ToEmail, IsMonthToDate
	)
	VALUES (
		'RPT_STATUS_CHANGED', 'Collection Status Changed', 'RptStatusChanged',
		1, 0, 0,
		'Id,Fullname,Username,Time Stamp,Old Status,New Status,Balance',
	    '#Id,Fullname,Username,TimeStamp,OldStatus,NewStatus,Balance',
		'emma@ezbob.com,nimrodk@ezbob.com', 0
	)

	INSERT INTO ReportsUsersMap (UserID, ReportID)
	SELECT
		u.Id,
		r.Id
	FROM
		ReportUsers u,
		ReportScheduler r
	WHERE
		u.UserName IN ('alexbo', 'stasd', 'nimrodk', 'eilaya', 'emanuellea')
		AND
		r.Type = 'RPT_STATUS_CHANGED'
		AND NOT EXISTS (
			SELECT *
			FROM ReportsUsersMap
			WHERE UserID = u.Id
			AND ReportID = r.ID
		)
		
	DECLARE @ReportId INT  = (SELECT Id FROM ReportScheduler r WHERE r.Type = 'RPT_STATUS_CHANGED')
	DECLARE @ReportArgId INT = (SELECT Id FROM ReportArgumentNames WHERE Name = 'DateRange')
	INSERT INTO ReportArguments (ReportArgumentNameId, ReportId) VALUES (@ReportArgId,@ReportId)
END
GO

