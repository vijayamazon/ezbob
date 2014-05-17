IF NOT EXISTS (SELECT * FROM ReportScheduler WHERE Type = 'RPT_BROKER')
BEGIN
	INSERT INTO ReportScheduler(Type, Title, StoredProcedure, IsDaily, IsWeekly, IsMonthly, Header, Fields, ToEmail, IsMonthToDate)
	VALUES (
		'RPT_BROKER', 'Broker report', 'RptBroker', 0, 0, 0,
		'Name,Company,Mobile,Phone,Email,Sign up Date',
		'Name,Company,Mobile,Phone,Email,SignUpDate',
		'', 0
	)

	INSERT INTO ReportArguments(ReportArgumentNameId, ReportId)
	SELECT
		1, Id
	FROM
		ReportScheduler
	WHERE
		Type = 'RPT_BROKER'
END
GO
