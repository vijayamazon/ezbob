IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ReportScheduler_ReportID]') AND parent_object_id = OBJECT_ID(N'[dbo].[ReportsUsersMap]'))
ALTER TABLE [dbo].[ReportsUsersMap] DROP CONSTRAINT [FK_ReportScheduler_ReportID]
GO
ALTER TABLE [dbo].[ReportsUsersMap]  WITH CHECK ADD  CONSTRAINT [FK_ReportScheduler_ReportID] FOREIGN KEY([ReportID])
REFERENCES [dbo].[ReportScheduler] ([Id])
GO
ALTER TABLE [dbo].[ReportsUsersMap] CHECK CONSTRAINT [FK_ReportScheduler_ReportID]
GO
