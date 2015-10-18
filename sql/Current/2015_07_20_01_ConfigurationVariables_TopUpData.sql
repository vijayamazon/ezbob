SET QUOTED_IDENTIFIER ON
GO

DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF @Environment = 'Prod'
BEGIN
	IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'TwilioSendingNumberIsrael')
	BEGIN
		INSERT INTO ConfigurationVariables (Name, Value, [Description], IsEncrypted) VALUES (
			'TwilioSendingNumberIsrael', '+972526285470', 'Twilio`s Israel from number', NULL)
	END
	
	IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'PacnetDublinEmails')
	BEGIN
		INSERT INTO ConfigurationVariables (Name, Value, [Description], IsEncrypted) VALUES (
			'PacnetDublinEmails', 'staff_snn_PacNet_ePayments@pacnetservices.com; epayments@eu.pacnetservices.com', 'Pacnet Irish office Emails for money transfer confirmation', NULL)
	END
	
	IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'PacnetVancouverEmails')
	BEGIN
		INSERT INTO ConfigurationVariables (Name, Value, [Description], IsEncrypted) VALUES (
			'PacnetVancouverEmails', 'ruth@pacnetservices.com; selene@pacnetservices.com', 'Pacnet Vancouver office Emails for money transfer confirmation', NULL)
	END
	
	IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'MailBeeLicenseKey')
	BEGIN
		INSERT INTO ConfigurationVariables (Name, Value, [Description], IsEncrypted) VALUES (
			'MailBeeLicenseKey', 'MN700-E9212517212D217021D6F428312E-52A1', 'Mail Bee License Key to enable processing money transfer confirmation email from Pacnet', NULL)
	END
	
	IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'MailBeeLoginAddress')
	BEGIN
		INSERT INTO ConfigurationVariables (Name, Value, [Description], IsEncrypted) VALUES (
			'MailBeeLoginAddress', 'pacnetconfirmationreceiver@ezbob.com', 'Mail Bee Login Address to enable processing money transfer confirmation email from Pacnet', NULL)
	END
	
	IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'MailBeeLoginPassword')
	BEGIN
		INSERT INTO ConfigurationVariables (Name, Value, [Description], IsEncrypted) VALUES (
			'MailBeeLoginPassword', 'ezbob2012$', 'Mail Bee LoginPassword to enable processing money transfer confirmation email from Pacnet', NULL)
	END
	
	IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'MailBeeServer')
	BEGIN
		INSERT INTO ConfigurationVariables (Name, Value, [Description], IsEncrypted) VALUES (
			'MailBeeServer', 'imap.gmail.com', 'Mail Bee server name to enable processing money transfer confirmation email from Pacnet', NULL)
	END
	
	IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'MailBeePort')
	BEGIN
		INSERT INTO ConfigurationVariables (Name, Value, [Description], IsEncrypted) VALUES (
			'MailBeePort', '993', 'Mail Bee port number to enable processing money transfer confirmation email from Pacnet', NULL)
	END
	
	IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'MailBeeMailboxReconnectionIntervalSeconds')
	BEGIN
		INSERT INTO ConfigurationVariables (Name, Value, [Description], IsEncrypted) VALUES (
			'MailBeeMailboxReconnectionIntervalSeconds', '60', 'Mail Bee Mailbox Reconnection Interval', NULL)
	END
	
	IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'TopUpSendingEmail')
	BEGIN
		INSERT INTO ConfigurationVariables (Name, Value, [Description], IsEncrypted) VALUES (
			'TopUpSendingEmail', 'risk@ezbob.com', 'Top-up Sending Email', NULL)
	END		

	IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'MaxTimeToWaitForPacnetrConfirmation')
	BEGIN
		INSERT INTO ConfigurationVariables (Name, Value, [Description], IsEncrypted) VALUES (
			'MaxTimeToWaitForPacnetrConfirmation', '60', 'Maximum Time To Wait For Money Transfer Confirmation from Pacnet', NULL)
	END		
END

IF @Environment <> 'Prod'
BEGIN
	IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'TwilioSendingNumberIsrael')
	BEGIN
		INSERT INTO ConfigurationVariables (Name, Value, [Description], IsEncrypted) VALUES (
			'TwilioSendingNumberIsrael', '', 'Twilio`s Israel from number', NULL)
	END
	
	IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'PacnetDublinEmails')
	BEGIN
		INSERT INTO ConfigurationVariables (Name, Value, [Description], IsEncrypted) VALUES (
			'PacnetDublinEmails', '', 'Pacnet Irish office Emails for money transfer confirmation', NULL)
	END
	
	IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'PacnetVancouverEmails')
	BEGIN
		INSERT INTO ConfigurationVariables (Name, Value, [Description], IsEncrypted) VALUES (
			'PacnetVancouverEmails', '', 'Pacnet Vancouver office Emails for money transfer confirmation', NULL)
	END
	
	IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'MailBeeLicenseKey')
	BEGIN
		INSERT INTO ConfigurationVariables (Name, Value, [Description], IsEncrypted) VALUES (
			'MailBeeLicenseKey', '', 'Mail Bee License Key to enable processing money transfer confirmation email from Pacnet', NULL)
	END
	
	IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'MailBeeLoginAddress')
	BEGIN
		INSERT INTO ConfigurationVariables (Name, Value, [Description], IsEncrypted) VALUES (
			'MailBeeLoginAddress', '', 'Mail Bee Login Address to enable processing money transfer confirmation email from Pacnet', NULL)
	END
	
	IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'MailBeeLoginPassword')
	BEGIN
		INSERT INTO ConfigurationVariables (Name, Value, [Description], IsEncrypted) VALUES (
			'MailBeeLoginPassword', '', 'Mail Bee LoginPassword to enable processing money transfer confirmation email from Pacnet', NULL)
	END
	
	IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'MailBeeServer')
	BEGIN
		INSERT INTO ConfigurationVariables (Name, Value, [Description], IsEncrypted) VALUES (
			'MailBeeServer', '', 'Mail Bee server name to enable processing money transfer confirmation email from Pacnet', NULL)
	END
	
	IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'MailBeePort')
	BEGIN
		INSERT INTO ConfigurationVariables (Name, Value, [Description], IsEncrypted) VALUES (
			'MailBeePort', '', 'Mail Bee port number to enable processing money transfer confirmation email from Pacnet', NULL)
	END
	
	IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'MailBeeMailboxReconnectionIntervalSeconds')
	BEGIN
		INSERT INTO ConfigurationVariables (Name, Value, [Description], IsEncrypted) VALUES (
			'MailBeeMailboxReconnectionIntervalSeconds', '', 'Mail Bee Mailbox Reconnection Interval', NULL)
	END
	
	IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'TopUpSendingEmail')
	BEGIN
		INSERT INTO ConfigurationVariables (Name, Value, [Description], IsEncrypted) VALUES (
			'TopUpSendingEmail', '', 'Top-up Sending Email', NULL)
	END	
	
	IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'MaxTimeToWaitForPacnetrConfirmation')
	BEGIN
		INSERT INTO ConfigurationVariables (Name, Value, [Description], IsEncrypted) VALUES (
			'MaxTimeToWaitForPacnetrConfirmation', '', 'Maximum Time To Wait For Money Transfer Confirmation from Pacnet', NULL)
	END	
END
GO


