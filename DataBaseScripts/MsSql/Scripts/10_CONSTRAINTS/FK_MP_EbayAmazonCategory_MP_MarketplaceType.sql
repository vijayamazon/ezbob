IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_EbayAmazonCategory_MP_MarketplaceType]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_EbayAmazonCategory]'))
ALTER TABLE [dbo].[MP_EbayAmazonCategory] DROP CONSTRAINT [FK_MP_EbayAmazonCategory_MP_MarketplaceType]
GO
ALTER TABLE [dbo].[MP_EbayAmazonCategory]  WITH CHECK ADD  CONSTRAINT [FK_MP_EbayAmazonCategory_MP_MarketplaceType] FOREIGN KEY([MarketplaceTypeId])
REFERENCES [dbo].[MP_MarketplaceType] ([Id])
GO
ALTER TABLE [dbo].[MP_EbayAmazonCategory] CHECK CONSTRAINT [FK_MP_EbayAmazonCategory_MP_MarketplaceType]
GO
