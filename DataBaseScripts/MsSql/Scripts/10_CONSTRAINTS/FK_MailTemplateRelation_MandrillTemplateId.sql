IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MailTemplateRelation_MandrillTemplateId]') AND parent_object_id = OBJECT_ID(N'[dbo].[MailTemplateRelation]'))
ALTER TABLE [dbo].[MailTemplateRelation] DROP CONSTRAINT [FK_MailTemplateRelation_MandrillTemplateId]
GO
ALTER TABLE [dbo].[MailTemplateRelation]  WITH CHECK ADD  CONSTRAINT [FK_MailTemplateRelation_MandrillTemplateId] FOREIGN KEY([MandrillTemplateId])
REFERENCES [dbo].[MandrillTemplate] ([Id])
GO
ALTER TABLE [dbo].[MailTemplateRelation] CHECK CONSTRAINT [FK_MailTemplateRelation_MandrillTemplateId]
GO
