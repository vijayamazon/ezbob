IF NOT EXISTS(SELECT * FROM syscolumns WHERE name = 'IsDeleted' AND id = OBJECT_ID('MP_VatReturnRecords'))
	ALTER TABLE MP_VatReturnRecords ADD IsDeleted BIT NULL
GO
