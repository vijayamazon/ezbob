IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_AmazonOrderItemDetailCatgory_MP_EbayAmazonCategory]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_AmazonOrderItemDetailCatgory]'))
ALTER TABLE [dbo].[MP_AmazonOrderItemDetailCatgory] DROP CONSTRAINT [FK_MP_AmazonOrderItemDetailCatgory_MP_EbayAmazonCategory]
GO
ALTER TABLE [dbo].[MP_AmazonOrderItemDetailCatgory]  WITH CHECK ADD  CONSTRAINT [FK_MP_AmazonOrderItemDetailCatgory_MP_EbayAmazonCategory] FOREIGN KEY([EbayAmazonCategoryId])
REFERENCES [dbo].[MP_EbayAmazonCategory] ([Id])
GO
ALTER TABLE [dbo].[MP_AmazonOrderItemDetailCatgory] CHECK CONSTRAINT [FK_MP_AmazonOrderItemDetailCatgory_MP_EbayAmazonCategory]
GO
