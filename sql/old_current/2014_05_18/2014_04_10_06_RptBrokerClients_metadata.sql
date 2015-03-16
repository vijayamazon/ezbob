UPDATE ReportScheduler SET
	Header = 'Id,Wizard Step,Credit result,Full name,Email,Daytime phone,Mobile phone,Registration date,Broker Name,Broker phone,Broker,Test broker',
	Fields = '#Id,WizardStepTypeName,CreditResult,FullName,Name,DaytimePhone,MobilePhone,GreetingMailSentDate,ContactName,ContactMobile,FirmName,TestBroker'
WHERE
	Type = 'RPT_BROKER_CLIENTS'
GO
