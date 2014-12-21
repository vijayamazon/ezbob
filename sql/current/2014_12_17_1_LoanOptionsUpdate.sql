IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('LoanOptions') AND name = 'EmailSendingAllowed')
BEGIN
	ALTER TABLE LoanOptions ADD EmailSendingAllowed BIT NOT NULL DEFAULT(0)
	ALTER TABLE LoanOptions ADD MailSendingAllowed BIT NOT NULL DEFAULT(0)
	ALTER TABLE LoanOptions ADD SmsSendingAllowed BIT NOT NULL DEFAULT(0)
END	
GO

IF EXISTS (SELECT 1 FROM syscolumns WHERE id = OBJECT_ID('LoanOptions') AND name = 'StopSendingEmails')
BEGIN
	EXECUTE('UPDATE LoanOptions SET EmailSendingAllowed = 1-StopSendingEmails, MailSendingAllowed=1-StopSendingEmails,SmsSendingAllowed=1-StopSendingEmails')
	EXECUTE('ALTER TABLE LoanOptions DROP COLUMN StopSendingEmails')
END	
GO