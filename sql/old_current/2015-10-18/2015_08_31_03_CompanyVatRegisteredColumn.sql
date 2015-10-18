IF NOT EXISTS (SELECT * FROM syscolumns WHERE id=object_id('Company') AND name = 'VatRegistered')
BEGIN
	ALTER TABLE Company ADD VatRegistered BIT
END
GO