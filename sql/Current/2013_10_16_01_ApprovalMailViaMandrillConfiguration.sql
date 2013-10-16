DECLARE @TemplateId INT

IF EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME = 'Mandrill - Approval (1st time)')
BEGIN
	SELECT @TemplateId = Id FROM MandrillTemplate WHERE NAME = 'Mandrill - Approval (1st time)'
END
ELSE BEGIN
	INSERT INTO MandrillTemplate(Name) VALUES ('Mandrill - Approval (1st time)')
	SELECT @TemplateId = Id FROM MandrillTemplate WHERE NAME = 'Mandrill - Approval (1st time)'
END

UPDATE MailTemplateRelation SET MandrillTemplateId = @TemplateId WHERE InternalTemplateName = 'Congratulations you are qualified.docx'
GO

DECLARE @TemplateId INT

IF EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME = 'Mandrill - Approval (not 1st time)')
BEGIN
	SELECT @TemplateId = Id FROM MandrillTemplate WHERE NAME = 'Mandrill - Approval (not 1st time)'
END
ELSE BEGIN
	INSERT INTO MandrillTemplate(Name) VALUES ('Mandrill - Approval (not 1st time)')
	SELECT @TemplateId = Id FROM MandrillTemplate WHERE NAME = 'Mandrill - Approval (not 1st time)'
END

IF NOT EXISTS (SELECT 1 FROM MailTemplateRelation WHERE InternalTemplateName = 'Congratulations you are qualified - not first.docx')
BEGIN
	INSERT INTO MailTemplateRelation(InternalTemplateName, MandrillTemplateId) VALUES ('Congratulations you are qualified - not first.docx', @TemplateId)
END
GO
