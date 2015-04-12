IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'FinishWizardForApproved')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description) VALUES (
		'FinishWizardForApproved',
		'{"CustomerID":0,"DoSendEmail":false,"DoMain":true,"DoFraud":true,"NewCreditLineOption":"UpdateEverythingAndApplyAutoRules","AvoidAutoDecision":0,"IsUnderwriterForced":false,"FraudMode":"FullCheck"}',
		'JSON string. Specifies FinishWizard strategy configuration for customer who was approved prior to wizard completion.'
	)
END
GO
