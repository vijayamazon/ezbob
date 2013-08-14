IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ReportArgument_Name]') AND parent_object_id = OBJECT_ID(N'[dbo].[ReportArguments]'))
ALTER TABLE [dbo].[ReportArguments] DROP CONSTRAINT [FK_ReportArgument_Name]
GO
ALTER TABLE [dbo].[ReportArguments]  WITH CHECK ADD  CONSTRAINT [FK_ReportArgument_Name] FOREIGN KEY([ReportArgumentNameId])
REFERENCES [dbo].[ReportArgumentNames] ([Id])
GO
ALTER TABLE [dbo].[ReportArguments] CHECK CONSTRAINT [FK_ReportArgument_Name]
GO
