IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ExperianDefaultAccountsData_ServiceLogId]') AND parent_object_id = OBJECT_ID(N'[dbo].[ExperianDefaultAccountsData]'))
ALTER TABLE [dbo].[ExperianDefaultAccountsData] DROP CONSTRAINT [FK_ExperianDefaultAccountsData_ServiceLogId]
GO
ALTER TABLE [dbo].[ExperianDefaultAccountsData]  WITH CHECK ADD  CONSTRAINT [FK_ExperianDefaultAccountsData_ServiceLogId] FOREIGN KEY([ServiceLogId])
REFERENCES [dbo].[MP_ServiceLog] ([Id])
GO
ALTER TABLE [dbo].[ExperianDefaultAccountsData] CHECK CONSTRAINT [FK_ExperianDefaultAccountsData_ServiceLogId]
GO
