IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_CustomerMarketplaceUpdatingActionLog_MP_CustomerMarketPlaceUpdatingHistory]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_CustomerMarketplaceUpdatingActionLog]'))
ALTER TABLE [dbo].[MP_CustomerMarketplaceUpdatingActionLog] DROP CONSTRAINT [FK_MP_CustomerMarketplaceUpdatingActionLog_MP_CustomerMarketPlaceUpdatingHistory]
GO
ALTER TABLE [dbo].[MP_CustomerMarketplaceUpdatingActionLog]  WITH CHECK ADD  CONSTRAINT [FK_MP_CustomerMarketplaceUpdatingActionLog_MP_CustomerMarketPlaceUpdatingHistory] FOREIGN KEY([CustomerMarketplaceUpdatingHistoryRecordId])
REFERENCES [dbo].[MP_CustomerMarketPlaceUpdatingHistory] ([Id])
GO
ALTER TABLE [dbo].[MP_CustomerMarketplaceUpdatingActionLog] CHECK CONSTRAINT [FK_MP_CustomerMarketplaceUpdatingActionLog_MP_CustomerMarketPlaceUpdatingHistory]
GO
