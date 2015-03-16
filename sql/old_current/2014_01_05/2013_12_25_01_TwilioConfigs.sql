IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='IsSmsValidationActive')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('IsSmsValidationActive', 'False', 'Determines if we use captcha or sms to validate customer')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='TwilioAccountSid')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('TwilioAccountSid', 'ACcc682df6341371ee27ada6858025490b', 'Twilio account sid')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='TwilioAuthToken')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('TwilioAuthToken', 'fab0b8bd342443ff44497273b4ba2aa1', 'Twilio''s authentication token')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='NumberOfMobileCodeAttempts')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('NumberOfMobileCodeAttempts', '3', 'Max number of code generation attempts')
END
GO

