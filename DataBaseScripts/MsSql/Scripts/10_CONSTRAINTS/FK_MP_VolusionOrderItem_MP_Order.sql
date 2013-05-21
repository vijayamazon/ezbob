IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_VolusionOrderItem_MP_Order]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_VolusionOrderItem]'))
ALTER TABLE [dbo].[MP_VolusionOrderItem] DROP CONSTRAINT [FK_MP_VolusionOrderItem_MP_Order]
GO
ALTER TABLE [dbo].[MP_VolusionOrderItem]  WITH CHECK ADD  CONSTRAINT [FK_MP_VolusionOrderItem_MP_Order] FOREIGN KEY([OrderId])
REFERENCES [dbo].[MP_VolusionOrder] ([Id])
GO
ALTER TABLE [dbo].[MP_VolusionOrderItem] CHECK CONSTRAINT [FK_MP_VolusionOrderItem_MP_Order]
GO
