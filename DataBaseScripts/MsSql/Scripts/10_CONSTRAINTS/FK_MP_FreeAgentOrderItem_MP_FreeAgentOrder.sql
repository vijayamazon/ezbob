IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_FreeAgentOrderItem_MP_FreeAgentOrder]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_FreeAgentOrderItem]'))
ALTER TABLE [dbo].[MP_FreeAgentOrderItem] DROP CONSTRAINT [FK_MP_FreeAgentOrderItem_MP_FreeAgentOrder]
GO
ALTER TABLE [dbo].[MP_FreeAgentOrderItem]  WITH CHECK ADD  CONSTRAINT [FK_MP_FreeAgentOrderItem_MP_FreeAgentOrder] FOREIGN KEY([OrderId])
REFERENCES [dbo].[MP_FreeAgentOrder] ([Id])
GO
ALTER TABLE [dbo].[MP_FreeAgentOrderItem] CHECK CONSTRAINT [FK_MP_FreeAgentOrderItem_MP_FreeAgentOrder]
GO
