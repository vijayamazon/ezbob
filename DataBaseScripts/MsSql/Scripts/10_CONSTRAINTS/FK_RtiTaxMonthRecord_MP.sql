IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_RtiTaxMonthRecord_MP]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_RtiTaxMonthRecords]'))
ALTER TABLE [dbo].[MP_RtiTaxMonthRecords] DROP CONSTRAINT [FK_RtiTaxMonthRecord_MP]
GO
ALTER TABLE [dbo].[MP_RtiTaxMonthRecords]  WITH CHECK ADD  CONSTRAINT [FK_RtiTaxMonthRecord_MP] FOREIGN KEY([CustomerMarketPlaceId])
REFERENCES [dbo].[MP_CustomerMarketPlace] ([Id])
GO
ALTER TABLE [dbo].[MP_RtiTaxMonthRecords] CHECK CONSTRAINT [FK_RtiTaxMonthRecord_MP]
GO
