IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_VolusionOrder_MP_CustomerMarketPlace]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_VolusionOrder]'))
ALTER TABLE [dbo].[MP_VolusionOrder] DROP CONSTRAINT [FK_MP_VolusionOrder_MP_CustomerMarketPlace]
GO
ALTER TABLE [dbo].[MP_VolusionOrder]  WITH CHECK ADD  CONSTRAINT [FK_MP_VolusionOrder_MP_CustomerMarketPlace] FOREIGN KEY([CustomerMarketPlaceId])
REFERENCES [dbo].[MP_CustomerMarketPlace] ([Id])
GO
ALTER TABLE [dbo].[MP_VolusionOrder] CHECK CONSTRAINT [FK_MP_VolusionOrder_MP_CustomerMarketPlace]
GO
