IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_EbayTransaction_MP_OrderItem]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_EbayTransaction]'))
ALTER TABLE [dbo].[MP_EbayTransaction] DROP CONSTRAINT [FK_MP_EbayTransaction_MP_OrderItem]
GO
ALTER TABLE [dbo].[MP_EbayTransaction]  WITH CHECK ADD  CONSTRAINT [FK_MP_EbayTransaction_MP_OrderItem] FOREIGN KEY([OrderItemId])
REFERENCES [dbo].[MP_EbayOrderItem] ([Id])
GO
ALTER TABLE [dbo].[MP_EbayTransaction] CHECK CONSTRAINT [FK_MP_EbayTransaction_MP_OrderItem]
GO
