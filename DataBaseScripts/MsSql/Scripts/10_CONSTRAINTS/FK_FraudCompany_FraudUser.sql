IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_FraudCompany_FraudUser]') AND parent_object_id = OBJECT_ID(N'[dbo].[FraudCompany]'))
ALTER TABLE [dbo].[FraudCompany] DROP CONSTRAINT [FK_FraudCompany_FraudUser]
GO
ALTER TABLE [dbo].[FraudCompany]  WITH CHECK ADD  CONSTRAINT [FK_FraudCompany_FraudUser] FOREIGN KEY([FraudUserId])
REFERENCES [dbo].[FraudUser] ([Id])
GO
ALTER TABLE [dbo].[FraudCompany] CHECK CONSTRAINT [FK_FraudCompany_FraudUser]
GO
