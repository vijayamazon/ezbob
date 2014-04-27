
UPDATE dbo.ReportScheduler
SET Header = 'Id,Status,Full name,Email,Daytime phone,Mobile phone,Registration date,Broker Name,Broker phone,Broker,Test broker'
	, Fields = '#Id,Status,FullName,Name,DaytimePhone,MobilePhone,GreetingMailSentDate,ContactName,ContactMobile,FirmName,TestBroker'
WHERE Type = 'RPT_BROKER_CLIENTS'
GO
