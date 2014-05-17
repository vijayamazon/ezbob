UPDATE ReportScheduler SET
	Header = 'Id,Reg Date,Credit Result,Email,Fullname,Wizard Step,Daytime Phone,Mobile Phone,Overall TurnOver,Segment,Reference Source',
	Fields = '#Id,GreetingMailSentDate,CreditResult,Name,Fullname,WizardStepTypeDescription,DaytimePhone,MobilePhone,OverallTurnOver,Segment,ReferenceSource'
WHERE
	Type = 'RPT_LEADS'
GO
