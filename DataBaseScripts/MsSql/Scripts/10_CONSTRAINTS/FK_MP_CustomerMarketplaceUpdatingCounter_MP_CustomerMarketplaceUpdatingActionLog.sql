IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_CustomerMarketplaceUpdatingCounter_MP_CustomerMarketplaceUpdatingActionLog]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_CustomerMarketplaceUpdatingCounter]'))
ALTER TABLE [dbo].[MP_CustomerMarketplaceUpdatingCounter] DROP CONSTRAINT [FK_MP_CustomerMarketplaceUpdatingCounter_MP_CustomerMarketplaceUpdatingActionLog]
GO
ALTER TABLE [dbo].[MP_CustomerMarketplaceUpdatingCounter]  WITH CHECK ADD  CONSTRAINT [FK_MP_CustomerMarketplaceUpdatingCounter_MP_CustomerMarketplaceUpdatingActionLog] FOREIGN KEY([CustomerMarketplaceUpdatingActionLogId])
REFERENCES [dbo].[MP_CustomerMarketplaceUpdatingActionLog] ([Id])
GO
ALTER TABLE [dbo].[MP_CustomerMarketplaceUpdatingCounter] CHECK CONSTRAINT [FK_MP_CustomerMarketplaceUpdatingCounter_MP_CustomerMarketplaceUpdatingActionLog]
GO
