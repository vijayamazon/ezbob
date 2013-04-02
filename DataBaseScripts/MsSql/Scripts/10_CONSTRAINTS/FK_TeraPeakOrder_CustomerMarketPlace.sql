IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_TeraPeakOrder_CustomerMarketPlace]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_TeraPeakOrder]'))
ALTER TABLE [dbo].[MP_TeraPeakOrder] DROP CONSTRAINT [FK_TeraPeakOrder_CustomerMarketPlace]
GO
ALTER TABLE [dbo].[MP_TeraPeakOrder]  WITH CHECK ADD  CONSTRAINT [FK_TeraPeakOrder_CustomerMarketPlace] FOREIGN KEY([CustomerMarketPlaceId])
REFERENCES [dbo].[MP_CustomerMarketPlace] ([Id])
GO
ALTER TABLE [dbo].[MP_TeraPeakOrder] CHECK CONSTRAINT [FK_TeraPeakOrder_CustomerMarketPlace]
GO
