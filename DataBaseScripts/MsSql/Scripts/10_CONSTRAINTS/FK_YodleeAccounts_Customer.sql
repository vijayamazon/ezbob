IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_YodleeAccounts_Customer]') AND parent_object_id = OBJECT_ID(N'[dbo].[YodleeAccounts]'))
ALTER TABLE [dbo].[YodleeAccounts] DROP CONSTRAINT [FK_YodleeAccounts_Customer]
GO
ALTER TABLE [dbo].[YodleeAccounts]  WITH CHECK ADD  CONSTRAINT [FK_YodleeAccounts_Customer] FOREIGN KEY([CustomerId])
REFERENCES [dbo].[Customer] ([Id])
GO
ALTER TABLE [dbo].[YodleeAccounts] CHECK CONSTRAINT [FK_YodleeAccounts_Customer]
GO
