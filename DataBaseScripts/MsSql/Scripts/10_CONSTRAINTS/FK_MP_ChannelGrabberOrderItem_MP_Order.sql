IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_ChannelGrabberOrderItem_MP_Order]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_ChannelGrabberOrderItem]'))
ALTER TABLE [dbo].[MP_ChannelGrabberOrderItem] DROP CONSTRAINT [FK_MP_ChannelGrabberOrderItem_MP_Order]
GO
ALTER TABLE [dbo].[MP_ChannelGrabberOrderItem]  WITH CHECK ADD  CONSTRAINT [FK_MP_ChannelGrabberOrderItem_MP_Order] FOREIGN KEY([OrderId])
REFERENCES [dbo].[MP_ChannelGrabberOrder] ([Id])
GO
ALTER TABLE [dbo].[MP_ChannelGrabberOrderItem] CHECK CONSTRAINT [FK_MP_ChannelGrabberOrderItem_MP_Order]
GO
