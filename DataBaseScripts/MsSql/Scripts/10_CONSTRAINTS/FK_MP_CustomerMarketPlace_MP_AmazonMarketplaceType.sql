IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_CustomerMarketPlace_MP_AmazonMarketplaceType]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_CustomerMarketPlace]'))
ALTER TABLE [dbo].[MP_CustomerMarketPlace] DROP CONSTRAINT [FK_MP_CustomerMarketPlace_MP_AmazonMarketplaceType]
GO
ALTER TABLE [dbo].[MP_CustomerMarketPlace]  WITH CHECK ADD  CONSTRAINT [FK_MP_CustomerMarketPlace_MP_AmazonMarketplaceType] FOREIGN KEY([AmazonMarketPlaceId])
REFERENCES [dbo].[MP_AmazonMarketplaceType] ([Id])
GO
ALTER TABLE [dbo].[MP_CustomerMarketPlace] CHECK CONSTRAINT [FK_MP_CustomerMarketPlace_MP_AmazonMarketplaceType]
GO
