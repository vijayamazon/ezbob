IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_TeraPeakOrder_MP_CustomerMarketPlaceUpdatingHistory]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_TeraPeakOrder]'))
ALTER TABLE [dbo].[MP_TeraPeakOrder] DROP CONSTRAINT [FK_MP_TeraPeakOrder_MP_CustomerMarketPlaceUpdatingHistory]
GO
ALTER TABLE [dbo].[MP_TeraPeakOrder]  WITH CHECK ADD  CONSTRAINT [FK_MP_TeraPeakOrder_MP_CustomerMarketPlaceUpdatingHistory] FOREIGN KEY([CustomerMarketPlaceUpdatingHistoryRecordId])
REFERENCES [dbo].[MP_CustomerMarketPlaceUpdatingHistory] ([Id])
GO
ALTER TABLE [dbo].[MP_TeraPeakOrder] CHECK CONSTRAINT [FK_MP_TeraPeakOrder_MP_CustomerMarketPlaceUpdatingHistory]
GO
