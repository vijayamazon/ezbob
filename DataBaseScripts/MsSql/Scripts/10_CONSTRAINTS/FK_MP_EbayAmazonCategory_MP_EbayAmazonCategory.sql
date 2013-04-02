IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_EbayAmazonCategory_MP_EbayAmazonCategory]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_EbayAmazonCategory]'))
ALTER TABLE [dbo].[MP_EbayAmazonCategory] DROP CONSTRAINT [FK_MP_EbayAmazonCategory_MP_EbayAmazonCategory]
GO
ALTER TABLE [dbo].[MP_EbayAmazonCategory]  WITH CHECK ADD  CONSTRAINT [FK_MP_EbayAmazonCategory_MP_EbayAmazonCategory] FOREIGN KEY([ParentId])
REFERENCES [dbo].[MP_EbayAmazonCategory] ([Id])
GO
ALTER TABLE [dbo].[MP_EbayAmazonCategory] CHECK CONSTRAINT [FK_MP_EbayAmazonCategory_MP_EbayAmazonCategory]
GO
