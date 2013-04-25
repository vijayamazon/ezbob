IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ReportUsers_UserID]') AND parent_object_id = OBJECT_ID(N'[dbo].[ReportsUsersMap]'))
ALTER TABLE [dbo].[ReportsUsersMap] DROP CONSTRAINT [FK_ReportUsers_UserID]
GO
ALTER TABLE [dbo].[ReportsUsersMap]  WITH CHECK ADD  CONSTRAINT [FK_ReportUsers_UserID] FOREIGN KEY([UserID])
REFERENCES [dbo].[ReportUsers] ([Id])
GO
ALTER TABLE [dbo].[ReportsUsersMap] CHECK CONSTRAINT [FK_ReportUsers_UserID]
GO
