DECLARE @TemplateId INT

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME = 'Mandrill - Approval Offline (1st time)')
BEGIN
	INSERT INTO MandrillTemplate(Name) VALUES ('Mandrill - Approval Offline (1st time)')
END

SELECT @TemplateId = Id FROM MandrillTemplate WHERE NAME = 'Mandrill - Approval Offline (1st time)'

IF NOT EXISTS (SELECT 1 FROM  MailTemplateRelation WHERE InternalTemplateName = 'Congratulations you are qualified - offline.docx')
BEGIN
	INSERT INTO MailTemplateRelation (InternalTemplateName, MandrillTemplateId) VALUES ('Congratulations you are qualified - offline.docx', @TemplateId)
END

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME = 'Mandrill - Approval Offline (not 1st time)')
BEGIN
	INSERT INTO MandrillTemplate(Name) VALUES ('Mandrill - Approval Offline (not 1st time)')
END

SELECT @TemplateId = Id FROM MandrillTemplate WHERE NAME = 'Mandrill - Approval Offline (not 1st time)'

IF NOT EXISTS (SELECT 1 FROM  MailTemplateRelation WHERE InternalTemplateName = 'Congratulations you are qualified - offline - not first.docx')
BEGIN
	INSERT INTO MailTemplateRelation (InternalTemplateName, MandrillTemplateId) VALUES ('Congratulations you are qualified - offline - not first.docx', @TemplateId)
END

GO




