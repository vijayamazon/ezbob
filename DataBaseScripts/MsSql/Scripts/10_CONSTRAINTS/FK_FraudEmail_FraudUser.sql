IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_FraudEmail_FraudUser]') AND parent_object_id = OBJECT_ID(N'[dbo].[FraudEmail]'))
ALTER TABLE [dbo].[FraudEmail] DROP CONSTRAINT [FK_FraudEmail_FraudUser]
GO
ALTER TABLE [dbo].[FraudEmail]  WITH CHECK ADD  CONSTRAINT [FK_FraudEmail_FraudUser] FOREIGN KEY([FraudUserId])
REFERENCES [dbo].[FraudUser] ([Id])
GO
ALTER TABLE [dbo].[FraudEmail] CHECK CONSTRAINT [FK_FraudEmail_FraudUser]
GO
