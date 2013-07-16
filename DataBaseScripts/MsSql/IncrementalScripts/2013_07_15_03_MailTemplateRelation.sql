IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MailTemplateRelation]') AND type in (N'U'))
DROP TABLE [dbo].[MailTemplateRelation]
GO

CREATE TABLE MailTemplateRelation(
	[Id] [int] IDENTITY(1,1) NOT NULL,
	InternalTemplateName NVARCHAR(200)  NOT NULL, 
	MandrillTemplateId INT NOT NULL 
)
GO

ALTER TABLE MailTemplateRelation  WITH CHECK ADD  CONSTRAINT [FK_MailTemplateRelation_MandrillTemplateId] FOREIGN KEY(MandrillTemplateId)
REFERENCES MandrillTemplate (Id)
GO

INSERT INTO MandrillTemplate VALUES ('Greeting');
INSERT INTO MailTemplateRelation(InternalTemplateName, MandrillTemplateId) VALUES ('Thanks for joining us.docx',@@Identity);