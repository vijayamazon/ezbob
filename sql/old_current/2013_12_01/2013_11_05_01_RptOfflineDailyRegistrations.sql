IF  NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptOfflineDailyRegistrations]') AND type in (N'P', N'PC'))
	EXECUTE('CREATE PROCEDURE [dbo].[RptOfflineDailyRegistrations] AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptOfflineDailyRegistrations
@DateStart DATE,
@DateEnd DATE
AS
	SELECT Id, Name, GreetingMailSentDate, WizardStep
	FROM Customer 
	WHERE GreetingMailSentDate >= @DateStart 
	AND GreetingMailSentDate < @DateEnd 
	AND IsOffline = 1 
	AND IsTest = 0 
	ORDER BY WizardStep desc
GO

IF NOT EXISTS (SELECT 1 FROM ReportScheduler WHERE Type = 'RPT_OFFLINE_DAILY_REGISTRATIONS') 
BEGIN
	INSERT INTO dbo.ReportScheduler (
		Type, Title, StoredProcedure,
		IsDaily, IsWeekly, IsMonthly,
		Header,
		Fields,
		ToEmail, IsMonthToDate
	)
	VALUES (
		'RPT_OFFLINE_DAILY_REGISTRATIONS', 'Offline Daily Registrations', 'RptOfflineDailyRegistrations',
		1, 0, 0,
		'Id,Name,GreetingMailSentDate,WizardStep',
		'Id,Name,GreetingMailSentDate,WizardStep',
		'ops@ezbob.com,gilada@ezbob.com', 0
	)

	INSERT INTO ReportsUsersMap (UserID, ReportID)
	SELECT
		u.Id,
		r.Id
	FROM
		ReportUsers u,
		ReportScheduler r
	WHERE
		u.UserName IN ('alexbo', 'stasd', 'nimrodk')
		AND
		r.Type = 'RPT_OFFLINE_DAILY_REGISTRATIONS'
		AND NOT EXISTS (
			SELECT *
			FROM ReportsUsersMap
			WHERE UserID = u.Id
			AND ReportID = r.ID
		)
		
	DECLARE @ReportId INT  = (SELECT Id FROM ReportScheduler r WHERE r.Type = 'RPT_OFFLINE_DAILY_REGISTRATIONS')
	DECLARE @ReportArgId INT = (SELECT Id FROM ReportArgumentNames WHERE Name = 'DateRange')
	INSERT INTO ReportArguments (ReportArgumentNameId, ReportId) VALUES (@ReportArgId,@ReportId)
END
GO

