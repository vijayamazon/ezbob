IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Application_Attachment_Application_Detail]') AND parent_object_id = OBJECT_ID(N'[dbo].[Application_Attachment]'))
ALTER TABLE [dbo].[Application_Attachment] DROP CONSTRAINT [FK_Application_Attachment_Application_Detail]
GO
ALTER TABLE [dbo].[Application_Attachment]  WITH NOCHECK ADD  CONSTRAINT [FK_Application_Attachment_Application_Detail] FOREIGN KEY([DetailId])
REFERENCES [dbo].[Application_Detail] ([DetailId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Application_Attachment] CHECK CONSTRAINT [FK_Application_Attachment_Application_Detail]
GO
