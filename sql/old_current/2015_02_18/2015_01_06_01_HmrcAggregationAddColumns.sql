
-- ADD FIELD ValueAdded to HmrcAggregation TABLE
IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('HmrcAggregation') AND name = 'ValueAdded')
BEGIN
	ALTER TABLE HmrcAggregation ADD ValueAdded NUMERIC (18, 2) DEFAULT (0) NOT NULL 
END
GO

-- ADD FIELD FreeCashFlow to HmrcAggregation TABLE
IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('HmrcAggregation') AND name = 'FreeCashFlow')
BEGIN
	ALTER TABLE HmrcAggregation ADD FreeCashFlow  NUMERIC (18, 2) DEFAULT (0) NOT NULL 
END
GO

-- REMOVE RECORDS WITHOUT PRIMARY TABLE IDs (MP_VatReturnEntries not linked properly to MP_VatReturnRecords)
DELETE FROM [dbo].[MP_VatReturnEntries] WHERE [RecordId] NOT IN (SELECT Id FROM [dbo].[MP_VatReturnRecords]);

-- ADDED FK VatReturnEntry => VatReturnRecord
IF OBJECT_ID('FK_VatReturnEntry_Record') IS NULL
BEGIN
	ALTER TABLE [dbo].[MP_VatReturnEntries] WITH CHECK ADD CONSTRAINT [FK_VatReturnEntry_Record] FOREIGN KEY ([RecordId]) REFERENCES [dbo].[MP_VatReturnRecords] ([Id])
	ALTER TABLE [dbo].[MP_VatReturnEntries] CHECK CONSTRAINT [FK_VatReturnEntry_Record]
END
GO

-- ADDED FK VatReturnEntry => VatReturnEntryName
IF OBJECT_ID('FK_VatReturnEntry_EntryName') IS NULL
BEGIN
	ALTER TABLE [dbo].[MP_VatReturnEntries] WITH CHECK ADD CONSTRAINT [FK_VatReturnEntry_EntryName] FOREIGN KEY ([NameId]) REFERENCES [dbo].[MP_VatReturnEntryNames] ([Id])
	ALTER TABLE [dbo].[MP_VatReturnEntries] CHECK CONSTRAINT [FK_VatReturnEntry_EntryName]
END
GO