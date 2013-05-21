IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_PayPointOrderItem_MP_PayPointOrder]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_PayPointOrderItem]'))
ALTER TABLE [dbo].[MP_PayPointOrderItem] DROP CONSTRAINT [FK_MP_PayPointOrderItem_MP_PayPointOrder]
GO
ALTER TABLE [dbo].[MP_PayPointOrderItem]  WITH CHECK ADD  CONSTRAINT [FK_MP_PayPointOrderItem_MP_PayPointOrder] FOREIGN KEY([OrderId])
REFERENCES [dbo].[MP_PayPointOrder] ([Id])
GO
ALTER TABLE [dbo].[MP_PayPointOrderItem] CHECK CONSTRAINT [FK_MP_PayPointOrderItem_MP_PayPointOrder]
GO
