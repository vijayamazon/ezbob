IF NOT EXISTS (SELECT * FROM syscolumns WHERE id=object_id('MP_CompanyFilesMetaData') AND name = 'IsBankStatement')
BEGIN
	ALTER TABLE MP_CompanyFilesMetaData ADD IsBankStatement BIT
END
GO