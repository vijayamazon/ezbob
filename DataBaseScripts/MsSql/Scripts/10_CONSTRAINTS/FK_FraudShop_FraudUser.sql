IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_FraudShop_FraudUser]') AND parent_object_id = OBJECT_ID(N'[dbo].[FraudShop]'))
ALTER TABLE [dbo].[FraudShop] DROP CONSTRAINT [FK_FraudShop_FraudUser]
GO
ALTER TABLE [dbo].[FraudShop]  WITH CHECK ADD  CONSTRAINT [FK_FraudShop_FraudUser] FOREIGN KEY([FraudUserId])
REFERENCES [dbo].[FraudUser] ([Id])
GO
ALTER TABLE [dbo].[FraudShop] CHECK CONSTRAINT [FK_FraudShop_FraudUser]
GO
