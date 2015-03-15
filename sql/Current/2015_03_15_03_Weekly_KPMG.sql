SET QUOTED_IDENTIFIER ON
GO

SELECT
	ActionName AS Name
INTO
	#stra
FROM
	EzServiceActionName
WHERE
	1 = 0

INSERT INTO #stra (Name) VALUES ('Ezbob.Backend.Strategies.Tasks.WeeklyMaamMedalAndPricing')

IF NOT EXISTS (SELECT * FROM EzServiceActionName n INNER JOIN #stra s ON n.ActionName = s.Name)
BEGIN
	INSERT INTO EzServiceActionName (ActionName)
	SELECT
		Name
	FROM
		#stra
END
GO

DECLARE @JobID BIGINT

IF NOT EXISTS (SELECT * FROM EzServiceCrontab c INNER JOIN EzServiceActionName n ON c.ActionNameID = n.ActionNameID INNER JOIN #stra s ON n.ActionName = s.Name)
BEGIN
	INSERT INTO EzServiceCrontab (ActionNameID, IsEnabled, RepetitionTypeID, RepetitionTime)
	SELECT
		n.ActionNameID,
		1,
		t.RepetitionTypeID,
		'March 14 2015 02:00:00'
	FROM
		EzServiceActionName n
		INNER JOIN #stra s ON n.ActionName = s.Name
		INNER JOIN EzServiceCronjobRepetitionTypes t ON t.RepetitionType = 'Daily'

	SET @JobID = SCOPE_IDENTITY()

	INSERT INTO EzServiceCronjobArguments (JobID, SerialNo, ArgumentTypeID, Value)
	SELECT
		@JobID,
		0,
		t.TypeID,
		'false'
	FROM
		EzServiceCronjobArgumentTypes t
	WHERE
		TypeName = 'bool'
		AND
		IsNullable = 0
END
GO

DROP TABLE #stra
GO

IF NOT EXISTS (SELECT * FROM ReportScheduler WHERE Type = 'RPT_WEEKLY_KPMG')
BEGIN
	INSERT INTO ReportScheduler (Type, Title, StoredProcedure, IsDaily, IsWeekly, IsMonthly, Header, Fields, ToEmail, IsMonthToDate)
	VALUES ('RPT_WEEKLY_KPMG', 'Weekly KPMG report', '', 0, 0, 0, '&nbsp;', 'dummy', 'alexbo+rptdev@ezbob.com', 0)
END
GO
