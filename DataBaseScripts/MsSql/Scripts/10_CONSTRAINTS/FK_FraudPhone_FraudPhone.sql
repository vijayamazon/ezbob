IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_FraudPhone_FraudPhone]') AND parent_object_id = OBJECT_ID(N'[dbo].[FraudPhone]'))
ALTER TABLE [dbo].[FraudPhone] DROP CONSTRAINT [FK_FraudPhone_FraudPhone]
GO
ALTER TABLE [dbo].[FraudPhone]  WITH CHECK ADD  CONSTRAINT [FK_FraudPhone_FraudPhone] FOREIGN KEY([FraudUserId])
REFERENCES [dbo].[FraudUser] ([Id])
GO
ALTER TABLE [dbo].[FraudPhone] CHECK CONSTRAINT [FK_FraudPhone_FraudPhone]
GO
