IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='FirstOfMonthStatusMailCopyTo')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('FirstOfMonthStatusMailCopyTo', '', 'Email defined here will get a copy of the first of month status emails')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='FirstOfMonthStatusMailMandrillTemplateName')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('FirstOfMonthStatusMailMandrillTemplateName', 'CustomerLoanStatus', 'Name of mandrill template for the first of month status mail')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='FirstOfMonthEnableCustomerMail')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('FirstOfMonthEnableCustomerMail', 0, 'If enabled customer will receive the status mail, otherwise the logic will still be executed but only the copy will be sent (if it exists)')
END

GO
