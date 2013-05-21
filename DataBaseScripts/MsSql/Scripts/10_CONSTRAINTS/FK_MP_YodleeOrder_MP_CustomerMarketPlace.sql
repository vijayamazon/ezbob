IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_YodleeOrder_MP_CustomerMarketPlace]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_YodleeOrder]'))
ALTER TABLE [dbo].[MP_YodleeOrder] DROP CONSTRAINT [FK_MP_YodleeOrder_MP_CustomerMarketPlace]
GO
ALTER TABLE [dbo].[MP_YodleeOrder]  WITH CHECK ADD  CONSTRAINT [FK_MP_YodleeOrder_MP_CustomerMarketPlace] FOREIGN KEY([CustomerMarketPlaceId])
REFERENCES [dbo].[MP_CustomerMarketPlace] ([Id])
GO
ALTER TABLE [dbo].[MP_YodleeOrder] CHECK CONSTRAINT [FK_MP_YodleeOrder_MP_CustomerMarketPlace]
GO
