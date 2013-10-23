IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='FirstOfMonthStatusMailCopyTo')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('FirstOfMonthStatusMailCopyTo', '', 'Email defined here will get a copy of the first of month status emails')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='FirstOfMonthStatusMailMandrillTemplateName')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('FirstOfMonthStatusMailMandrillTemplateName', 'CustomerLoanStatus', 'Name of mandrill template for the first of month status mail')
END

GO
