IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_AmazonOrderItem2_MP_AmazonOrder]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_AmazonOrderItem2]'))
ALTER TABLE [dbo].[MP_AmazonOrderItem2] DROP CONSTRAINT [FK_MP_AmazonOrderItem2_MP_AmazonOrder]
GO
ALTER TABLE [dbo].[MP_AmazonOrderItem2]  WITH CHECK ADD  CONSTRAINT [FK_MP_AmazonOrderItem2_MP_AmazonOrder] FOREIGN KEY([AmazonOrderId])
REFERENCES [dbo].[MP_AmazonOrder] ([Id])
GO
ALTER TABLE [dbo].[MP_AmazonOrderItem2] CHECK CONSTRAINT [FK_MP_AmazonOrderItem2_MP_AmazonOrder]
GO
