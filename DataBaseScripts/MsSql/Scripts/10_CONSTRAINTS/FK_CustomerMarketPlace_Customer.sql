IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_CustomerMarketPlace_Customer]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_CustomerMarketPlace]'))
ALTER TABLE [dbo].[MP_CustomerMarketPlace] DROP CONSTRAINT [FK_CustomerMarketPlace_Customer]
GO
ALTER TABLE [dbo].[MP_CustomerMarketPlace]  WITH CHECK ADD  CONSTRAINT [FK_CustomerMarketPlace_Customer] FOREIGN KEY([CustomerId])
REFERENCES [dbo].[Customer] ([Id])
GO
ALTER TABLE [dbo].[MP_CustomerMarketPlace] CHECK CONSTRAINT [FK_CustomerMarketPlace_Customer]
GO
