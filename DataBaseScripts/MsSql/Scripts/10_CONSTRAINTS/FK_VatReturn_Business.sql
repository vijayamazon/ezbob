IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_VatReturn_Business]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_VatReturnRecords]'))
ALTER TABLE [dbo].[MP_VatReturnRecords] DROP CONSTRAINT [FK_VatReturn_Business]
GO
ALTER TABLE [dbo].[MP_VatReturnRecords]  WITH CHECK ADD  CONSTRAINT [FK_VatReturn_Business] FOREIGN KEY([BusinessId])
REFERENCES [dbo].[Business] ([Id])
GO
ALTER TABLE [dbo].[MP_VatReturnRecords] CHECK CONSTRAINT [FK_VatReturn_Business]
GO
