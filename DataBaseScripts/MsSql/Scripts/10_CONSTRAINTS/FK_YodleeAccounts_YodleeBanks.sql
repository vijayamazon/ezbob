IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_YodleeAccounts_YodleeBanks]') AND parent_object_id = OBJECT_ID(N'[dbo].[YodleeAccounts]'))
ALTER TABLE [dbo].[YodleeAccounts] DROP CONSTRAINT [FK_YodleeAccounts_YodleeBanks]
GO
ALTER TABLE [dbo].[YodleeAccounts]  WITH CHECK ADD  CONSTRAINT [FK_YodleeAccounts_YodleeBanks] FOREIGN KEY([BankId])
REFERENCES [dbo].[YodleeBanks] ([Id])
GO
ALTER TABLE [dbo].[YodleeAccounts] CHECK CONSTRAINT [FK_YodleeAccounts_YodleeBanks]
GO
