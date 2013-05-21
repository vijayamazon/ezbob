IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_YodleeOrderItem_MP_YodleeOrder]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_YodleeOrderItem]'))
ALTER TABLE [dbo].[MP_YodleeOrderItem] DROP CONSTRAINT [FK_MP_YodleeOrderItem_MP_YodleeOrder]
GO
ALTER TABLE [dbo].[MP_YodleeOrderItem]  WITH CHECK ADD  CONSTRAINT [FK_MP_YodleeOrderItem_MP_YodleeOrder] FOREIGN KEY([OrderId])
REFERENCES [dbo].[MP_YodleeOrder] ([Id])
GO
ALTER TABLE [dbo].[MP_YodleeOrderItem] CHECK CONSTRAINT [FK_MP_YodleeOrderItem_MP_YodleeOrder]
GO
