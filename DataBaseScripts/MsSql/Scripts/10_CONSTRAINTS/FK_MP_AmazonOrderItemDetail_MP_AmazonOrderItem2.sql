IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_AmazonOrderItemDetail_MP_AmazonOrderItem2]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_AmazonOrderItemDetail]'))
ALTER TABLE [dbo].[MP_AmazonOrderItemDetail] DROP CONSTRAINT [FK_MP_AmazonOrderItemDetail_MP_AmazonOrderItem2]
GO
ALTER TABLE [dbo].[MP_AmazonOrderItemDetail]  WITH CHECK ADD  CONSTRAINT [FK_MP_AmazonOrderItemDetail_MP_AmazonOrderItem2] FOREIGN KEY([OrderItem2Id])
REFERENCES [dbo].[MP_AmazonOrderItem2] ([Id])
GO
ALTER TABLE [dbo].[MP_AmazonOrderItemDetail] CHECK CONSTRAINT [FK_MP_AmazonOrderItemDetail_MP_AmazonOrderItem2]
GO
