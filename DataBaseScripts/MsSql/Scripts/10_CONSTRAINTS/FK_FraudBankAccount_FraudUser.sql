IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_FraudBankAccount_FraudUser]') AND parent_object_id = OBJECT_ID(N'[dbo].[FraudBankAccount]'))
ALTER TABLE [dbo].[FraudBankAccount] DROP CONSTRAINT [FK_FraudBankAccount_FraudUser]
GO
ALTER TABLE [dbo].[FraudBankAccount]  WITH CHECK ADD  CONSTRAINT [FK_FraudBankAccount_FraudUser] FOREIGN KEY([FraudUserId])
REFERENCES [dbo].[FraudUser] ([Id])
GO
ALTER TABLE [dbo].[FraudBankAccount] CHECK CONSTRAINT [FK_FraudBankAccount_FraudUser]
GO
