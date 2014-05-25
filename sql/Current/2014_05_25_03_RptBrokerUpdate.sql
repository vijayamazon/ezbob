UPDATE dbo.ReportScheduler
SET Header = 'Name,Company,Mobile,Phone,Email,Signup Date,Num of clients added, Num of loans given, Value of loans given, Value of commission paid, Source Ref'
	, Fields = 'Name,Company,Mobile,Phone,Email,SignUpDate,NumOfClients,NumOfLoans,ValueOfLoans,ValueOfCommission,SourceRef'
WHERE Type = 'RPT_BROKER'
GO
