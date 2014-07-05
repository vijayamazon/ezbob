IF NOT EXISTS (SELECT 1 FROM ReportScheduler WHERE Type = 'RPT_CURRENT_LIENS') 
BEGIN
	INSERT INTO dbo.ReportScheduler (
		Type, Title, StoredProcedure,
		IsDaily, IsWeekly, IsMonthly,
		Header,
		Fields,
		ToEmail, IsMonthToDate
	)
	VALUES (
		'RPT_CURRENT_LIENS', 'Current Liens', 'RptCurrentLiens',
		0, 0, 0,
		'Loans,Principal,Lien Company',
	    'Loans,Principal,LienCompany',
		'', 0
	)

	INSERT INTO ReportsUsersMap (UserID, ReportID)
	SELECT
		u.Id,
		r.Id
	FROM
		ReportUsers u,
		ReportScheduler r
	WHERE
		u.UserName IN ('stasd', 'nimrodk', 'eilaya')
		AND
		r.Type = 'RPT_CURRENT_LIENS'
		AND NOT EXISTS (
			SELECT *
			FROM ReportsUsersMap
			WHERE UserID = u.Id
			AND ReportID = r.ID
		)
		
	DECLARE @ReportId INT  = (SELECT Id FROM ReportScheduler r WHERE r.Type = 'RPT_CURRENT_LIENS')
	DECLARE @ReportArgId INT = (SELECT Id FROM ReportArgumentNames WHERE Name = 'DateRange')
	INSERT INTO ReportArguments (ReportArgumentNameId, ReportId) VALUES (@ReportArgId,@ReportId)
END
GO

IF NOT EXISTS (SELECT 1 FROM ReportScheduler WHERE Type = 'RPT_PAYMENT_TO_LIENS_LOANS') 
BEGIN
	INSERT INTO dbo.ReportScheduler (
		Type, Title, StoredProcedure,
		IsDaily, IsWeekly, IsMonthly,
		Header,
		Fields,
		ToEmail, IsMonthToDate
	)
	VALUES (
		'RPT_PAYMENT_TO_LIENS_LOANS', 'Payment to lien loans', 'RptPaymentToLiensLoans',
		0, 0, 0,
		'Loans, Amount, Lien Company',
	    'Loans,Amount,Name',
		'', 0
	)

	INSERT INTO ReportsUsersMap (UserID, ReportID)
	SELECT
		u.Id,
		r.Id
	FROM
		ReportUsers u,
		ReportScheduler r
	WHERE
		u.UserName IN ('stasd', 'nimrodk', 'eilaya')
		AND
		r.Type = 'RPT_PAYMENT_TO_LIENS_LOANS'
		AND NOT EXISTS (
			SELECT *
			FROM ReportsUsersMap
			WHERE UserID = u.Id
			AND ReportID = r.ID
		)
		
	DECLARE @ReportId INT  = (SELECT Id FROM ReportScheduler r WHERE r.Type = 'RPT_PAYMENT_TO_LIENS_LOANS')
	DECLARE @ReportArgId INT = (SELECT Id FROM ReportArgumentNames WHERE Name = 'DateRange')
	INSERT INTO ReportArguments (ReportArgumentNameId, ReportId) VALUES (@ReportArgId,@ReportId)
END
GO
