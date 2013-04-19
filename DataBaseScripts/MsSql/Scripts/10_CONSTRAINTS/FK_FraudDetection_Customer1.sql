IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_FraudDetection_Customer1]') AND parent_object_id = OBJECT_ID(N'[dbo].[FraudDetection]'))
ALTER TABLE [dbo].[FraudDetection] DROP CONSTRAINT [FK_FraudDetection_Customer1]
GO
ALTER TABLE [dbo].[FraudDetection]  WITH CHECK ADD  CONSTRAINT [FK_FraudDetection_Customer1] FOREIGN KEY([InternalCustomerId])
REFERENCES [dbo].[Customer] ([Id])
GO
ALTER TABLE [dbo].[FraudDetection] CHECK CONSTRAINT [FK_FraudDetection_Customer1]
GO
