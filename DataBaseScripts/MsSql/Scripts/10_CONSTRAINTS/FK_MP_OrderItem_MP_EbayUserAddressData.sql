IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_OrderItem_MP_EbayUserAddressData]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_EbayOrderItem]'))
ALTER TABLE [dbo].[MP_EbayOrderItem] DROP CONSTRAINT [FK_MP_OrderItem_MP_EbayUserAddressData]
GO
ALTER TABLE [dbo].[MP_EbayOrderItem]  WITH CHECK ADD  CONSTRAINT [FK_MP_OrderItem_MP_EbayUserAddressData] FOREIGN KEY([ShippingAddressId])
REFERENCES [dbo].[MP_EbayUserAddressData] ([Id])
GO
ALTER TABLE [dbo].[MP_EbayOrderItem] CHECK CONSTRAINT [FK_MP_OrderItem_MP_EbayUserAddressData]
GO
