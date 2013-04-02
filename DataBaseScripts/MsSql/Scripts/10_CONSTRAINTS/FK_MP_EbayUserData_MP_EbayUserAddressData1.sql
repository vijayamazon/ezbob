IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_EbayUserData_MP_EbayUserAddressData1]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_EbayUserData]'))
ALTER TABLE [dbo].[MP_EbayUserData] DROP CONSTRAINT [FK_MP_EbayUserData_MP_EbayUserAddressData1]
GO
ALTER TABLE [dbo].[MP_EbayUserData]  WITH CHECK ADD  CONSTRAINT [FK_MP_EbayUserData_MP_EbayUserAddressData1] FOREIGN KEY([SellerInfoSellerPaymentAddressId])
REFERENCES [dbo].[MP_EbayUserAddressData] ([Id])
GO
ALTER TABLE [dbo].[MP_EbayUserData] CHECK CONSTRAINT [FK_MP_EbayUserData_MP_EbayUserAddressData1]
GO
