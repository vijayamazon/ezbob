IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_OrderItem_MP_Order]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_EbayOrderItem]'))
ALTER TABLE [dbo].[MP_EbayOrderItem] DROP CONSTRAINT [FK_MP_OrderItem_MP_Order]
GO
ALTER TABLE [dbo].[MP_EbayOrderItem]  WITH CHECK ADD  CONSTRAINT [FK_MP_OrderItem_MP_Order] FOREIGN KEY([OrderId])
REFERENCES [dbo].[MP_EbayOrder] ([Id])
GO
ALTER TABLE [dbo].[MP_EbayOrderItem] CHECK CONSTRAINT [FK_MP_OrderItem_MP_Order]
GO
