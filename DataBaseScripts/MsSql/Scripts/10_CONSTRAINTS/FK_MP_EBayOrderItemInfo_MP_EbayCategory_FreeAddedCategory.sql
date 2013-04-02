IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_EBayOrderItemInfo_MP_EbayCategory_FreeAddedCategory]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_EBayOrderItemDetail]'))
ALTER TABLE [dbo].[MP_EBayOrderItemDetail] DROP CONSTRAINT [FK_MP_EBayOrderItemInfo_MP_EbayCategory_FreeAddedCategory]
GO
ALTER TABLE [dbo].[MP_EBayOrderItemDetail]  WITH CHECK ADD  CONSTRAINT [FK_MP_EBayOrderItemInfo_MP_EbayCategory_FreeAddedCategory] FOREIGN KEY([FreeAddedCategoryId])
REFERENCES [dbo].[MP_EbayAmazonCategory] ([Id])
GO
ALTER TABLE [dbo].[MP_EBayOrderItemDetail] CHECK CONSTRAINT [FK_MP_EBayOrderItemInfo_MP_EbayCategory_FreeAddedCategory]
GO
