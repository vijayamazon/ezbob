DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='ImailUserName')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('ImailUserName', 'Emma123456', 'imail user name')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='IMailPassword')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('IMailPassword', 'Ezbob123', 'imail password')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='IMailDebugModeEnabled')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('IMailDebugModeEnabled', 'True', 'If true - instead of mail email is sent to defined email')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='IMailSavePath')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('IMailSavePath', 'c:\temp\imail', 'If true - sms is not sent automatically ')
END


IF @Environment = 'Dev' OR @Environment IS NULL 
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='IMailDebugModeEmail')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('IMailDebugModeEmail', '', 'if set email and debug mode enabled the email will be sent to this address instead of mail to customer')
	END
END

IF @Environment = 'QA' OR @Environment = 'UAT'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='IMailDebugModeEmail')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('IMailDebugModeEmail', 'qa@ezbob.com', 'if set email and debug mode enabled the email will be sent to this address instead of mail to customer')
	END
END

IF @Environment = 'Prod'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='IMailDebugModeEmail')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('IMailDebugModeEmail', 'it@ezbob.com', 'if set email and debug mode enabled the email will be sent to this address instead of mail to customer')
	END
END
GO