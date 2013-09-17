IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='LateBy14Days')
BEGIN
	INSERT INTO MandrillTemplate VALUES ('LateBy14Days')
END
GO

IF NOT EXISTS (SELECT 1 FROM MailTemplateRelation WHERE InternalTemplateName='Late by 14 days.docx')
BEGIN
	DECLARE @TemplateId INT
	SELECT @TemplateId = Id FROM MandrillTemplate WHERE NAME='LateBy14Days'
	
	INSERT INTO MailTemplateRelation VALUES ('Late by 14 days.docx', @TemplateId)
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name = 'LateBy14DaysMailSendViaMandrill')
BEGIN
	INSERT INTO ConfigurationVariables VALUES ('LateBy14DaysMailSendViaMandrill', 'Yes', 'Enable sending late by 14 days mail with Mandrill')
END
GO

