IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_AmazonOrder_CustomerMarketPlace]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_AmazonOrder]'))
ALTER TABLE [dbo].[MP_AmazonOrder] DROP CONSTRAINT [FK_AmazonOrder_CustomerMarketPlace]
GO
ALTER TABLE [dbo].[MP_AmazonOrder]  WITH CHECK ADD  CONSTRAINT [FK_AmazonOrder_CustomerMarketPlace] FOREIGN KEY([CustomerMarketPlaceId])
REFERENCES [dbo].[MP_CustomerMarketPlace] ([Id])
GO
ALTER TABLE [dbo].[MP_AmazonOrder] CHECK CONSTRAINT [FK_AmazonOrder_CustomerMarketPlace]
GO
