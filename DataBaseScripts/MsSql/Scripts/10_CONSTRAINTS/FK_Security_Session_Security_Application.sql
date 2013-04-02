IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Security_Session_Security_Application]') AND parent_object_id = OBJECT_ID(N'[dbo].[Security_Session]'))
ALTER TABLE [dbo].[Security_Session] DROP CONSTRAINT [FK_Security_Session_Security_Application]
GO
ALTER TABLE [dbo].[Security_Session]  WITH NOCHECK ADD  CONSTRAINT [FK_Security_Session_Security_Application] FOREIGN KEY([AppId])
REFERENCES [dbo].[Security_Application] ([ApplicationId])
GO
ALTER TABLE [dbo].[Security_Session] CHECK CONSTRAINT [FK_Security_Session_Security_Application]
GO
