IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_TeraPeakOrderItem_MP_TeraPeakOrder]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_TeraPeakOrderItem]'))
ALTER TABLE [dbo].[MP_TeraPeakOrderItem] DROP CONSTRAINT [FK_MP_TeraPeakOrderItem_MP_TeraPeakOrder]
GO
ALTER TABLE [dbo].[MP_TeraPeakOrderItem]  WITH CHECK ADD  CONSTRAINT [FK_MP_TeraPeakOrderItem_MP_TeraPeakOrder] FOREIGN KEY([TeraPeakOrderId])
REFERENCES [dbo].[MP_TeraPeakOrder] ([Id])
GO
ALTER TABLE [dbo].[MP_TeraPeakOrderItem] CHECK CONSTRAINT [FK_MP_TeraPeakOrderItem_MP_TeraPeakOrder]
GO
