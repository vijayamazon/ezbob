DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF @Environment = 'Dev' OR @Environment IS NULL 
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AutomationExplanationMailReciever')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AutomationExplanationMailReciever', '', 'An expalnation email to this address will be sent when automatic decision differs from verification decision')
	END
END	

IF @Environment = 'QA' OR @Environment = 'UAT'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AutomationExplanationMailReciever')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AutomationExplanationMailReciever', 'eilaya+automation@ezbob.com', 'An expalnation email to this address will be sent when automatic decision differs from verification decision')
	END
END

IF @Environment = 'Prod'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AutomationExplanationMailReciever')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AutomationExplanationMailReciever', 'dev+automation@ezbob.com', 'An expalnation email to this address will be sent when automatic decision differs from verification decision')
	END
END

IF @Environment = 'Dev' OR @Environment IS NULL 
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='MailSenderEmail')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('MailSenderEmail', 'dev@ezbob.com', 'email for sender for templateless emails sent via mandrill')
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('MailSenderName', 'Dev Mailer', 'name for sender for templateless emails sent via mandrill')
	END
END	

IF @Environment = 'QA' OR @Environment = 'UAT'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='MailSenderEmail')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('MailSenderEmail', 'qa@ezbob.com', 'email for sender for templateless emails sent via mandrill')
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('MailSenderName', 'QA/UAT Mailer', 'name for sender for templateless emails sent via mandrill')
	END
END

IF @Environment = 'Prod'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='MailSenderEmail')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('MailSenderEmail', 'ezbob@ezbob.com', 'email for sender for templateless emails sent via mandrill')
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('MailSenderName', 'ezbob', 'name for sender for templateless emails sent via mandrill')
	END
END

GO
