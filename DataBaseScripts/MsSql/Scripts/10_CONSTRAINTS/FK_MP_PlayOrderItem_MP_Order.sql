IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_PlayOrderItem_MP_Order]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_PlayOrderItem]'))
ALTER TABLE [dbo].[MP_PlayOrderItem] DROP CONSTRAINT [FK_MP_PlayOrderItem_MP_Order]
GO
ALTER TABLE [dbo].[MP_PlayOrderItem]  WITH CHECK ADD  CONSTRAINT [FK_MP_PlayOrderItem_MP_Order] FOREIGN KEY([OrderId])
REFERENCES [dbo].[MP_PlayOrder] ([Id])
GO
ALTER TABLE [dbo].[MP_PlayOrderItem] CHECK CONSTRAINT [FK_MP_PlayOrderItem_MP_Order]
GO
