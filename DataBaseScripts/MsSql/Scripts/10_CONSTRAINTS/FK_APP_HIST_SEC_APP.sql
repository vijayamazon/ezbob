IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_APP_HIST_SEC_APP]') AND parent_object_id = OBJECT_ID(N'[dbo].[Application_History]'))
ALTER TABLE [dbo].[Application_History] DROP CONSTRAINT [FK_APP_HIST_SEC_APP]
GO
ALTER TABLE [dbo].[Application_History]  WITH NOCHECK ADD  CONSTRAINT [FK_APP_HIST_SEC_APP] FOREIGN KEY([SecurityApplicationId])
REFERENCES [dbo].[Security_Application] ([ApplicationId])
GO
ALTER TABLE [dbo].[Application_History] CHECK CONSTRAINT [FK_APP_HIST_SEC_APP]
GO
