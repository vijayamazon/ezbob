UPDATE dbo.ReportScheduler
SET Header = 'Broker Id,Name,Company,Mobile,Phone,Email,Signup Date,Num of clients added, Num of loans given, Value of loans given, Value of commission paid, Source Ref',
    Fields = '^BrokerID,Name,Company,Mobile,Phone,Email,SignUpDate,NumOfClients,NumOfLoans,ValueOfLoans,ValueOfCommission,SourceRef'
WHERE Type='RPT_BROKER'
GO


UPDATE dbo.ReportScheduler
SET Header = 'Id,Status,Full name,Email,Daytime phone,Mobile phone,Registration date,Broker Id,Broker Name,Broker phone,Broker,Approved amount, Taken amount'
	, Fields = '#Id,Status,FullName,Name,DaytimePhone,MobilePhone,GreetingMailSentDate,^BrokerID,ContactName,ContactMobile,FirmName,ApprovedAmount, TakenAmount'
WHERE Type='RPT_BROKER_CLIENTS'
GO