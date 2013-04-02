IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_APP_HIST_APP_APP]') AND parent_object_id = OBJECT_ID(N'[dbo].[Application_History]'))
ALTER TABLE [dbo].[Application_History] DROP CONSTRAINT [FK_APP_HIST_APP_APP]
GO
ALTER TABLE [dbo].[Application_History]  WITH NOCHECK ADD  CONSTRAINT [FK_APP_HIST_APP_APP] FOREIGN KEY([ApplicationId])
REFERENCES [dbo].[Application_Application] ([ApplicationId])
GO
ALTER TABLE [dbo].[Application_History] CHECK CONSTRAINT [FK_APP_HIST_APP_APP]
GO
