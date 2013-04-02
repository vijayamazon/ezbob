IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_CustomerMarketPlace_MarketPlace]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_CustomerMarketPlace]'))
ALTER TABLE [dbo].[MP_CustomerMarketPlace] DROP CONSTRAINT [FK_CustomerMarketPlace_MarketPlace]
GO
ALTER TABLE [dbo].[MP_CustomerMarketPlace]  WITH CHECK ADD  CONSTRAINT [FK_CustomerMarketPlace_MarketPlace] FOREIGN KEY([MarketPlaceId])
REFERENCES [dbo].[MP_MarketplaceType] ([Id])
GO
ALTER TABLE [dbo].[MP_CustomerMarketPlace] CHECK CONSTRAINT [FK_CustomerMarketPlace_MarketPlace]
GO
