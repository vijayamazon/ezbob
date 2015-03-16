IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'IsDeleted' AND id = OBJECT_ID('MP_VatReturnEntries'))
	ALTER TABLE MP_VatReturnEntries ADD IsDeleted BIT NULL
GO
