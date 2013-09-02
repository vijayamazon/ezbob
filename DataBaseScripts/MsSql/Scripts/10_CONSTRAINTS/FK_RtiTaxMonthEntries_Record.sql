IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_RtiTaxMonthEntries_Record]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_RtiTaxMonthEntries]'))
ALTER TABLE [dbo].[MP_RtiTaxMonthEntries] DROP CONSTRAINT [FK_RtiTaxMonthEntries_Record]
GO
ALTER TABLE [dbo].[MP_RtiTaxMonthEntries]  WITH CHECK ADD  CONSTRAINT [FK_RtiTaxMonthEntries_Record] FOREIGN KEY([RecordId])
REFERENCES [dbo].[MP_RtiTaxMonthRecords] ([Id])
GO
ALTER TABLE [dbo].[MP_RtiTaxMonthEntries] CHECK CONSTRAINT [FK_RtiTaxMonthEntries_Record]
GO
