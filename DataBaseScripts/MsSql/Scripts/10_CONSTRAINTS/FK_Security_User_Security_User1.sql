IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Security_User_Security_User1]') AND parent_object_id = OBJECT_ID(N'[dbo].[Security_User]'))
ALTER TABLE [dbo].[Security_User] DROP CONSTRAINT [FK_Security_User_Security_User1]
GO
ALTER TABLE [dbo].[Security_User]  WITH NOCHECK ADD  CONSTRAINT [FK_Security_User_Security_User1] FOREIGN KEY([DeleteUserId])
REFERENCES [dbo].[Security_User] ([UserId])
GO
ALTER TABLE [dbo].[Security_User] CHECK CONSTRAINT [FK_Security_User_Security_User1]
GO
