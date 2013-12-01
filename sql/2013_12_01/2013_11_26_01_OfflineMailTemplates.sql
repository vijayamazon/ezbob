IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - Took Offline Loan (1st loan)')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - Took Offline Loan (1st loan)')
	INSERT INTO MailTemplateRelation (InternalTemplateName, MandrillTemplateId) VALUES ('Get cash - approval offline.docx', @@IDENTITY)
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - Took Offline Loan (not 1st loan)')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - Took Offline Loan (not 1st loan)')
	INSERT INTO MailTemplateRelation (InternalTemplateName, MandrillTemplateId) VALUES ('Get cash - approval offline - not first.docx', @@IDENTITY)
END
GO








