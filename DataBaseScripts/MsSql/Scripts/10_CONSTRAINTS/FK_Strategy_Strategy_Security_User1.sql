IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Strategy_Strategy_Security_User1]') AND parent_object_id = OBJECT_ID(N'[dbo].[Strategy_Strategy]'))
ALTER TABLE [dbo].[Strategy_Strategy] DROP CONSTRAINT [FK_Strategy_Strategy_Security_User1]
GO
ALTER TABLE [dbo].[Strategy_Strategy]  WITH CHECK ADD  CONSTRAINT [FK_Strategy_Strategy_Security_User1] FOREIGN KEY([AuthorId])
REFERENCES [dbo].[Security_User] ([UserId])
GO
ALTER TABLE [dbo].[Strategy_Strategy] CHECK CONSTRAINT [FK_Strategy_Strategy_Security_User1]
GO
