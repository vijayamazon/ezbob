IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_FraudEmailDomain_FraudUser]') AND parent_object_id = OBJECT_ID(N'[dbo].[FraudEmailDomain]'))
ALTER TABLE [dbo].[FraudEmailDomain] DROP CONSTRAINT [FK_FraudEmailDomain_FraudUser]
GO
ALTER TABLE [dbo].[FraudEmailDomain]  WITH CHECK ADD  CONSTRAINT [FK_FraudEmailDomain_FraudUser] FOREIGN KEY([FraudUserId])
REFERENCES [dbo].[FraudUser] ([Id])
GO
ALTER TABLE [dbo].[FraudEmailDomain] CHECK CONSTRAINT [FK_FraudEmailDomain_FraudUser]
GO
