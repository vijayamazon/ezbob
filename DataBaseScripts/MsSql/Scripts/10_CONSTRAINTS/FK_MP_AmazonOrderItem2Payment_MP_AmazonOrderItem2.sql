IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_AmazonOrderItem2Payment_MP_AmazonOrderItem2]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_AmazonOrderItem2Payment]'))
ALTER TABLE [dbo].[MP_AmazonOrderItem2Payment] DROP CONSTRAINT [FK_MP_AmazonOrderItem2Payment_MP_AmazonOrderItem2]
GO
ALTER TABLE [dbo].[MP_AmazonOrderItem2Payment]  WITH CHECK ADD  CONSTRAINT [FK_MP_AmazonOrderItem2Payment_MP_AmazonOrderItem2] FOREIGN KEY([OrderItem2Id])
REFERENCES [dbo].[MP_AmazonOrderItem2] ([Id])
GO
ALTER TABLE [dbo].[MP_AmazonOrderItem2Payment] CHECK CONSTRAINT [FK_MP_AmazonOrderItem2Payment_MP_AmazonOrderItem2]
GO
