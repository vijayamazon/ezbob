IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_EbayTransaction_MP_EBayOrderItemInfo]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_EbayTransaction]'))
ALTER TABLE [dbo].[MP_EbayTransaction] DROP CONSTRAINT [FK_MP_EbayTransaction_MP_EBayOrderItemInfo]
GO
ALTER TABLE [dbo].[MP_EbayTransaction]  WITH CHECK ADD  CONSTRAINT [FK_MP_EbayTransaction_MP_EBayOrderItemInfo] FOREIGN KEY([ItemInfoId])
REFERENCES [dbo].[MP_EBayOrderItemDetail] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[MP_EbayTransaction] CHECK CONSTRAINT [FK_MP_EbayTransaction_MP_EBayOrderItemInfo]
GO
