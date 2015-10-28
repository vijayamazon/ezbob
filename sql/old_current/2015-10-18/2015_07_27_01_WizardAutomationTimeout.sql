SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'WizardAutomationTimeout')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES (
		'WizardAutomationTimeout', '20', 'Integer. Amount of seconds to wait for auto decision between wizard completion and transfering the customer to dashboard.', 0
	)
END
GO
