UPDATE ReportScheduler SET
	Header = 'Name,Company,Mobile,Phone,Email,Sign up Date,Test broker',
	Fields = 'Name,Company,Mobile,Phone,Email,SignUpDate,TestBroker'
WHERE
	Type = 'RPT_BROKER'
GO
