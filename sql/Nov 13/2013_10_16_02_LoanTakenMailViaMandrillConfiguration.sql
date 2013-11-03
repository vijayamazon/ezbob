DECLARE @TemplateId INT
IF EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME = 'Mandrill - Took Loan (1st loan)')
BEGIN
	SELECT @TemplateId = Id FROM MandrillTemplate WHERE NAME = 'Mandrill - Took Loan (1st loan)'
END
ELSE
BEGIN
	INSERT INTO MandrillTemplate (Name) VALUES ('Mandrill - Took Loan (1st loan)')
	SELECT @TemplateId = Id FROM MandrillTemplate WHERE NAME = 'Mandrill - Took Loan (1st loan)'
END

UPDATE MailTemplateRelation SET MandrillTemplateId = @TemplateId WHERE InternalTemplateName = 'Get cash - approval.docx'
GO

DECLARE @TemplateId INT
IF EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME = 'Mandrill - Took Loan (not 1st loan)')
BEGIN
	SELECT @TemplateId = Id FROM MandrillTemplate WHERE NAME = 'Mandrill - Took Loan (not 1st loan)'
END
ELSE
BEGIN
	INSERT INTO MandrillTemplate (Name) VALUES ('Mandrill - Took Loan (not 1st loan)')
	SELECT @TemplateId = Id FROM MandrillTemplate WHERE NAME = 'Mandrill - Took Loan (not 1st loan)'
END

IF NOT EXISTS (SELECT 1 FROM MailTemplateRelation WHERE InternalTemplateName = 'Get cash - approval - not first.docx')
BEGIN
	INSERT INTO MailTemplateRelation(InternalTemplateName, MandrillTemplateId) VALUES ('Get cash - approval - not first.docx', @TemplateId)
END
GO

