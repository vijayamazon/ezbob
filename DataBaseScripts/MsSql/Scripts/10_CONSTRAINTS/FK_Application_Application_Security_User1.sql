IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Application_Application_Security_User1]') AND parent_object_id = OBJECT_ID(N'[dbo].[Application_Application]'))
ALTER TABLE [dbo].[Application_Application] DROP CONSTRAINT [FK_Application_Application_Security_User1]
GO
ALTER TABLE [dbo].[Application_Application]  WITH NOCHECK ADD  CONSTRAINT [FK_Application_Application_Security_User1] FOREIGN KEY([CreatorUserId])
REFERENCES [dbo].[Security_User] ([UserId])
GO
ALTER TABLE [dbo].[Application_Application] CHECK CONSTRAINT [FK_Application_Application_Security_User1]
GO
