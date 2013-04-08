IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ExperianDefaultAccountsData_CustomerId]') AND parent_object_id = OBJECT_ID(N'[dbo].[ExperianDefaultAccountsData]'))
ALTER TABLE [dbo].[ExperianDefaultAccountsData] DROP CONSTRAINT [FK_ExperianDefaultAccountsData_CustomerId]
GO
ALTER TABLE [dbo].[ExperianDefaultAccountsData]  WITH CHECK ADD  CONSTRAINT [FK_ExperianDefaultAccountsData_CustomerId] FOREIGN KEY([CustomerId])
REFERENCES [dbo].[Customer] ([Id])
GO
ALTER TABLE [dbo].[ExperianDefaultAccountsData] CHECK CONSTRAINT [FK_ExperianDefaultAccountsData_CustomerId]
GO
