IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_InventoryItem_MP_Inventory]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_EbayAmazonInventoryItem]'))
ALTER TABLE [dbo].[MP_EbayAmazonInventoryItem] DROP CONSTRAINT [FK_MP_InventoryItem_MP_Inventory]
GO
ALTER TABLE [dbo].[MP_EbayAmazonInventoryItem]  WITH CHECK ADD  CONSTRAINT [FK_MP_InventoryItem_MP_Inventory] FOREIGN KEY([InventoryId])
REFERENCES [dbo].[MP_EbayAmazonInventory] ([Id])
GO
ALTER TABLE [dbo].[MP_EbayAmazonInventoryItem] CHECK CONSTRAINT [FK_MP_InventoryItem_MP_Inventory]
GO
