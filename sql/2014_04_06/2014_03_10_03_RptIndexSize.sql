IF NOT EXISTS (SELECT 1 FROM ReportScheduler WHERE Type = 'RPT_INDEX_SIZE') 
BEGIN
	INSERT INTO dbo.ReportScheduler (
		Type, Title, StoredProcedure,
		IsDaily, IsWeekly, IsMonthly,
		Header,
		Fields,
		ToEmail, IsMonthToDate
	)
	VALUES (
		'RPT_INDEX_SIZE', 'Index Size', 'RptIndexSize',
		0, 1, 1,
		'Table Name,Index Name,Current Reserved Page Size,Previous Resereved Page Size,Size Change',
	    'TableName,IndexName,CurrentReservedPageSize,PreviousReserevedPageSize,SizeChange',
		'operations@ezbob.com', 0
	)

	INSERT INTO ReportsUsersMap (UserID, ReportID)
	SELECT
		u.Id,
		r.Id
	FROM
		ReportUsers u,
		ReportScheduler r
	WHERE
		u.UserName IN ('alexbo', 'stasd', 'nimrodk', 'eilaya', 'yulys')
		AND
		r.Type = 'RPT_INDEX_SIZE'
		AND NOT EXISTS (
			SELECT *
			FROM ReportsUsersMap
			WHERE UserID = u.Id
			AND ReportID = r.ID
		)
		
	DECLARE @ReportId INT  = (SELECT Id FROM ReportScheduler r WHERE r.Type = 'RPT_INDEX_SIZE')
	DECLARE @ReportArgId INT = (SELECT Id FROM ReportArgumentNames WHERE Name = 'DateRange')
	INSERT INTO ReportArguments (ReportArgumentNameId, ReportId) VALUES (@ReportArgId,@ReportId)
END
GO


