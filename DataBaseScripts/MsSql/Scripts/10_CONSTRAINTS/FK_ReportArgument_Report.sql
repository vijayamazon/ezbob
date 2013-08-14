IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ReportArgument_Report]') AND parent_object_id = OBJECT_ID(N'[dbo].[ReportArguments]'))
ALTER TABLE [dbo].[ReportArguments] DROP CONSTRAINT [FK_ReportArgument_Report]
GO
ALTER TABLE [dbo].[ReportArguments]  WITH CHECK ADD  CONSTRAINT [FK_ReportArgument_Report] FOREIGN KEY([ReportId])
REFERENCES [dbo].[ReportScheduler] ([Id])
GO
ALTER TABLE [dbo].[ReportArguments] CHECK CONSTRAINT [FK_ReportArgument_Report]
GO
