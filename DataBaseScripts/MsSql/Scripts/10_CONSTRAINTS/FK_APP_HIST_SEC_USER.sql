IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_APP_HIST_SEC_USER]') AND parent_object_id = OBJECT_ID(N'[dbo].[Application_History]'))
ALTER TABLE [dbo].[Application_History] DROP CONSTRAINT [FK_APP_HIST_SEC_USER]
GO
ALTER TABLE [dbo].[Application_History]  WITH NOCHECK ADD  CONSTRAINT [FK_APP_HIST_SEC_USER] FOREIGN KEY([UserId])
REFERENCES [dbo].[Security_User] ([UserId])
GO
ALTER TABLE [dbo].[Application_History] CHECK CONSTRAINT [FK_APP_HIST_SEC_USER]
GO
