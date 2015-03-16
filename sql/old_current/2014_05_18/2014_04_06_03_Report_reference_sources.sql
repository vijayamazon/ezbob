IF NOT EXISTS (SELECT 1 FROM ReportScheduler WHERE Type = 'RPT_NEW_LOANS')
BEGIN
	INSERT INTO dbo.ReportScheduler
		(
		Type
		, Title
		, StoredProcedure
		, IsDaily
		, IsWeekly
		, IsMonthly
		, Header
		, Fields
		, ToEmail
		, IsMonthToDate
		)
	VALUES
		(
		'RPT_NEW_LOANS'
		, 'New Loans'
		, 'RptNewLoans'
		, 0
		, 0
		, 0
		, 'Customer Id, Loan Amount,Reference Source, Is Offline, Month, Year, Google Cookie, Date'
		, 'CustomerId,LoanAmount,ReferenceSource,IsOffline,MonthPart, YearPart, GoogleCookie, Date'
		, 'nimrodk@ezbob.com'
		, 0
		)
		
	INSERT INTO ReportArguments (ReportArgumentNameId, ReportID)
	SELECT
		1, 
		Id
	FROM
		ReportScheduler
	WHERE 
		Type = 'RPT_NEW_LOANS'
		
	DECLARE @UserId INT, @ReportId INT
	SELECT @UserId = Id FROM ReportUsers WHERE UserName = 'nimrodk'
	SELECT @ReportId = Id FROM ReportScheduler WHERE Type = 'RPT_NEW_LOANS'

	IF @UserId IS NOT NULL 
		INSERT INTO ReportsUsersMap (UserID, ReportID) VALUES (@UserId, @ReportId)

	SELECT @UserId = Id FROM ReportUsers WHERE UserName = 'yulys'
	IF @UserId IS NOT NULL 
		INSERT INTO ReportsUsersMap (UserID, ReportID) VALUES (@UserId, @ReportId)
END
GO

IF OBJECT_ID('RptReferenceSources') IS NULL
	EXECUTE('CREATE PROCEDURE RptReferenceSources AS SELECT 1')
GO
	