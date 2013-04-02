IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_AmazonOrderItemDetailCatgory_MP_AmazonOrderItemDetail]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_AmazonOrderItemDetailCatgory]'))
ALTER TABLE [dbo].[MP_AmazonOrderItemDetailCatgory] DROP CONSTRAINT [FK_MP_AmazonOrderItemDetailCatgory_MP_AmazonOrderItemDetail]
GO
ALTER TABLE [dbo].[MP_AmazonOrderItemDetailCatgory]  WITH CHECK ADD  CONSTRAINT [FK_MP_AmazonOrderItemDetailCatgory_MP_AmazonOrderItemDetail] FOREIGN KEY([AmazonOrderItemDetailId])
REFERENCES [dbo].[MP_AmazonOrderItemDetail] ([Id])
GO
ALTER TABLE [dbo].[MP_AmazonOrderItemDetailCatgory] CHECK CONSTRAINT [FK_MP_AmazonOrderItemDetailCatgory_MP_AmazonOrderItemDetail]
GO
