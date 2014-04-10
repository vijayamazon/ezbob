IF NOT EXISTS (SELECT * FROM ReportScheduler WHERE Type = 'RPT_BROKER_CLIENTS')
BEGIN
	INSERT INTO ReportScheduler(Type, Title, StoredProcedure, IsDaily, IsWeekly, IsMonthly, Header, Fields, ToEmail, IsMonthToDate)
	VALUES (
		'RPT_BROKER_CLIENTS', 'Broker clients', 'RptBrokerClients', 0, 0, 0,
		'Id,Wizard Step,Credit result,Full name,Email,Daytime phone,Mobile phone,Registration date,Broker Name,Broker phone,Broker',
		'#Id,WizardStepTypeName,CreditResult,FullName,Name,DaytimePhone,MobilePhone,GreetingMailSentDate,ContactName,ContactMobile,FirmName',
		'', 0
	)

	INSERT INTO ReportArguments(ReportArgumentNameId, ReportId)
	SELECT
		1, Id
	FROM
		ReportScheduler
	WHERE
		Type = 'RPT_BROKER_CLIENTS'
END
GO
