IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_EkmOrderItem_MP_Order]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_EkmOrderItem]'))
ALTER TABLE [dbo].[MP_EkmOrderItem] DROP CONSTRAINT [FK_MP_EkmOrderItem_MP_Order]
GO
ALTER TABLE [dbo].[MP_EkmOrderItem]  WITH CHECK ADD  CONSTRAINT [FK_MP_EkmOrderItem_MP_Order] FOREIGN KEY([OrderId])
REFERENCES [dbo].[MP_EkmOrder] ([Id])
GO
ALTER TABLE [dbo].[MP_EkmOrderItem] CHECK CONSTRAINT [FK_MP_EkmOrderItem_MP_Order]
GO
