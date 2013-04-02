IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_EbayExternalTransaction_MP_EbayOrderItem]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_EbayExternalTransaction]'))
ALTER TABLE [dbo].[MP_EbayExternalTransaction] DROP CONSTRAINT [FK_MP_EbayExternalTransaction_MP_EbayOrderItem]
GO
ALTER TABLE [dbo].[MP_EbayExternalTransaction]  WITH CHECK ADD  CONSTRAINT [FK_MP_EbayExternalTransaction_MP_EbayOrderItem] FOREIGN KEY([OrderItemId])
REFERENCES [dbo].[MP_EbayOrderItem] ([Id])
GO
ALTER TABLE [dbo].[MP_EbayExternalTransaction] CHECK CONSTRAINT [FK_MP_EbayExternalTransaction_MP_EbayOrderItem]
GO
