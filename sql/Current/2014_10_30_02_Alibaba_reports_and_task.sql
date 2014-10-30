IF NOT EXISTS (SELECT * FROM ReportScheduler WHERE Type = 'RPT_ALIBABA_DATA_SHARING')
BEGIN
	INSERT INTO ReportScheduler(Type, Title, StoredProcedure, IsDaily, IsWeekly, IsMonthly, Header, Fields, ToEmail, IsMonthToDate)
		VALUES ('RPT_ALIBABA_DATA_SHARING', 'Alibaba data sharing', 'none', 0, 0, 0, 'dummy', 'dummy', 'alexbo+rptdev@ezbob.com', 0)
END
GO

IF NOT EXISTS (SELECT * FROM ReportScheduler WHERE Type = 'RPT_ALIBABA_FUNNEL')
BEGIN
	INSERT INTO ReportScheduler(Type, Title, StoredProcedure, IsDaily, IsWeekly, IsMonthly, Header, Fields, ToEmail, IsMonthToDate)
		VALUES ('RPT_ALIBABA_FUNNEL', 'Alibaba funnel', 'none', 0, 0, 0, 'dummy', 'dummy', 'alexbo+rptdev@ezbob.com', 0)
END
GO

IF 1 < 0
BEGIN
	DECLARE @ActionNameID INT
	DECLARE @ActionName NVARCHAR(255)

	SET @ActionName = 'EzBob.Backend.Strategies.Tasks.DailyFetchGoogleAnalyticsAndRunDependent'

	IF NOT EXISTS (SELECT * FROM EzServiceActionName WHERE ActionName LIKE @ActionName)
		INSERT INTO EzServiceActionName (ActionName) VALUES (@ActionName)

	SELECT
		@ActionNameID = ActionNameID
	FROM
		EzServiceActionName
	WHERE
		ActionName LIKE @ActionName

	INSERT INTO EzServiceCrontab (ActionNameID, IsEnabled, RepetitionTypeID, RepetitionTime)
		VALUES(@ActionNameID, 1, 2, 'Oct 30 2014 0:10:0')
END
GO
