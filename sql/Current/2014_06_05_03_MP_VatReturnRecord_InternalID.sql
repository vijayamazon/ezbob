IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'InternalID' AND id = OBJECT_ID('MP_VatReturnRecords'))
BEGIN
	ALTER TABLE MP_VatReturnRecords ADD InternalID UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_VatReturnRecords_InternalID DEFAULT (NEWID())

	ALTER TABLE MP_VatReturnRecords DROP CONSTRAINT DF_VatReturnRecords_InternalID
END
GO
