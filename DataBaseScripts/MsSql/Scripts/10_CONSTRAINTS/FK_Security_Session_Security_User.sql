IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Security_Session_Security_User]') AND parent_object_id = OBJECT_ID(N'[dbo].[Security_Session]'))
ALTER TABLE [dbo].[Security_Session] DROP CONSTRAINT [FK_Security_Session_Security_User]
GO
ALTER TABLE [dbo].[Security_Session]  WITH CHECK ADD  CONSTRAINT [FK_Security_Session_Security_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[Security_User] ([UserId])
GO
ALTER TABLE [dbo].[Security_Session] CHECK CONSTRAINT [FK_Security_Session_Security_User]
GO
