IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_FraudAddress_FraudUser]') AND parent_object_id = OBJECT_ID(N'[dbo].[FraudAddress]'))
ALTER TABLE [dbo].[FraudAddress] DROP CONSTRAINT [FK_FraudAddress_FraudUser]
GO
ALTER TABLE [dbo].[FraudAddress]  WITH CHECK ADD  CONSTRAINT [FK_FraudAddress_FraudUser] FOREIGN KEY([FraudUserId])
REFERENCES [dbo].[FraudUser] ([Id])
GO
ALTER TABLE [dbo].[FraudAddress] CHECK CONSTRAINT [FK_FraudAddress_FraudUser]
GO
