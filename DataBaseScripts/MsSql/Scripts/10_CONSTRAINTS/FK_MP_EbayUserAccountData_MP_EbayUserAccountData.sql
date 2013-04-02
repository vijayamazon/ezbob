IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_EbayUserAccountData_MP_EbayUserAccountData]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_EbayUserAccountData]'))
ALTER TABLE [dbo].[MP_EbayUserAccountData] DROP CONSTRAINT [FK_MP_EbayUserAccountData_MP_EbayUserAccountData]
GO
ALTER TABLE [dbo].[MP_EbayUserAccountData]  WITH CHECK ADD  CONSTRAINT [FK_MP_EbayUserAccountData_MP_EbayUserAccountData] FOREIGN KEY([CustomerMarketPlaceId])
REFERENCES [dbo].[MP_CustomerMarketPlace] ([Id])
GO
ALTER TABLE [dbo].[MP_EbayUserAccountData] CHECK CONSTRAINT [FK_MP_EbayUserAccountData_MP_EbayUserAccountData]
GO
