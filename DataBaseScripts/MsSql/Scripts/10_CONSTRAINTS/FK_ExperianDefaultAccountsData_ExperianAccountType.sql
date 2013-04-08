IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ExperianDefaultAccountsData_ExperianAccountType]') AND parent_object_id = OBJECT_ID(N'[dbo].[ExperianDefaultAccountsData]'))
ALTER TABLE [dbo].[ExperianDefaultAccountsData] DROP CONSTRAINT [FK_ExperianDefaultAccountsData_ExperianAccountType]
GO
ALTER TABLE [dbo].[ExperianDefaultAccountsData]  WITH CHECK ADD  CONSTRAINT [FK_ExperianDefaultAccountsData_ExperianAccountType] FOREIGN KEY([AccountType])
REFERENCES [dbo].[ExperianAccountTypes] ([Id])
GO
ALTER TABLE [dbo].[ExperianDefaultAccountsData] CHECK CONSTRAINT [FK_ExperianDefaultAccountsData_ExperianAccountType]
GO
