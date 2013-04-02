IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_EbayUserAdditionalAccountData_MP_EbayUserAccountData]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_EbayUserAdditionalAccountData]'))
ALTER TABLE [dbo].[MP_EbayUserAdditionalAccountData] DROP CONSTRAINT [FK_MP_EbayUserAdditionalAccountData_MP_EbayUserAccountData]
GO
ALTER TABLE [dbo].[MP_EbayUserAdditionalAccountData]  WITH CHECK ADD  CONSTRAINT [FK_MP_EbayUserAdditionalAccountData_MP_EbayUserAccountData] FOREIGN KEY([EbayUserAccountDataId])
REFERENCES [dbo].[MP_EbayUserAccountData] ([Id])
GO
ALTER TABLE [dbo].[MP_EbayUserAdditionalAccountData] CHECK CONSTRAINT [FK_MP_EbayUserAdditionalAccountData_MP_EbayUserAccountData]
GO
