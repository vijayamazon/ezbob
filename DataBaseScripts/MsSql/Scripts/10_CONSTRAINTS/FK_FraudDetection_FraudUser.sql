IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_FraudDetection_FraudUser]') AND parent_object_id = OBJECT_ID(N'[dbo].[FraudDetection]'))
ALTER TABLE [dbo].[FraudDetection] DROP CONSTRAINT [FK_FraudDetection_FraudUser]
GO
ALTER TABLE [dbo].[FraudDetection]  WITH CHECK ADD  CONSTRAINT [FK_FraudDetection_FraudUser] FOREIGN KEY([ExternalUserId])
REFERENCES [dbo].[FraudUser] ([Id])
GO
ALTER TABLE [dbo].[FraudDetection] CHECK CONSTRAINT [FK_FraudDetection_FraudUser]
GO
