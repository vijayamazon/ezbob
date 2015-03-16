IF  NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptOnlineLeads]') AND type in (N'P', N'PC'))
	EXECUTE('CREATE PROCEDURE [dbo].[RptOnlineLeads] AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptOnlineLeads
@DateStart DATETIME,
@DateEnd DATETIME
AS
SELECT
	c.Id,
	c.GreetingMailSentDate,
	c.CreditResult,
	c.Name,
	c.Fullname,
	w.WizardStepTypeDescription,
	c.DaytimePhone,
	c.MobilePhone,
	c.OverallTurnOver
FROM
	Customer c
	INNER JOIN WizardStepTypes w ON c.WizardStep = w.WizardStepTypeID
WHERE
	@DateStart <= c.GreetingMailSentDate AND c.GreetingMailSentDate < @DateEnd
	AND
	c.IsTest = 0
	AND
	c.IsOffline = 0
ORDER BY
	w.TheLastOne DESC,
	w.WizardStepTypeDescription DESC,
	c.GreetingMailSentDate,
	c.Fullname

GO

IF NOT EXISTS (SELECT 1 FROM ReportScheduler WHERE Type = 'RPT_ONLINE_LEADS') 
BEGIN
	INSERT INTO dbo.ReportScheduler (
		Type, Title, StoredProcedure,
		IsDaily, IsWeekly, IsMonthly,
		Header,
		Fields,
		ToEmail, IsMonthToDate
	)
	VALUES (
		'RPT_ONLINE_LEADS', 'Online Leads', 'RptOnlineLeads',
		1, 0, 0,
		'Id,Reg Date,Credit Result,Email,Fullname,Wizard Step,Daytime Phone,Mobile Phone,Overall TurnOver',
	    '!Id,GreetingMailSentDate,CreditResult,Name,Fullname,WizardStepTypeDescription,DaytimePhone,MobilePhone,OverallTurnOver',
		'nimrodk@ezbob.com,ops@ezbob.com,rosb@ezbob.com', 0
	)

	INSERT INTO ReportsUsersMap (UserID, ReportID)
	SELECT
		u.Id,
		r.Id
	FROM
		ReportUsers u,
		ReportScheduler r
	WHERE
		u.UserName IN ('alexbo', 'stasd', 'nimrodk', 'eilaya', 'rosb')
		AND
		r.Type = 'RPT_ONLINE_LEADS'
		AND NOT EXISTS (
			SELECT *
			FROM ReportsUsersMap
			WHERE UserID = u.Id
			AND ReportID = r.ID
		)
		
	DECLARE @ReportId INT  = (SELECT Id FROM ReportScheduler r WHERE r.Type = 'RPT_ONLINE_LEADS')
	DECLARE @ReportArgId INT = (SELECT Id FROM ReportArgumentNames WHERE Name = 'DateRange')
	INSERT INTO ReportArguments (ReportArgumentNameId, ReportId) VALUES (@ReportArgId,@ReportId)
END
GO


