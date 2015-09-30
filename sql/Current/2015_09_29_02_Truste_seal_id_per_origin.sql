SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('CustomerOrigin') AND name = 'TimestampCounter')
	ALTER TABLE CustomerOrigin DROP COLUMN TimestampCounter
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('CustomerOrigin') AND name = 'TrusteSealUniqueID')
BEGIN
	ALTER TABLE CustomerOrigin ADD TrusteSealUniqueID UNIQUEIDENTIFIER NULL

	EXECUTE('
		UPDATE CustomerOrigin SET TrusteSealUniqueID = ''4787d133-f65d-4dfb-9edd-2980113986bc'' WHERE Name  = ''everline''
		UPDATE CustomerOrigin SET TrusteSealUniqueID = ''8b55a44a-6f1d-49a2-adc3-f7bc2b6169db'' WHERE Name != ''everline''
	')

	ALTER TABLE CustomerOrigin ALTER COLUMN TrusteSealUniqueID UNIQUEIDENTIFIER NOT NULL
END
GO

ALTER TABLE CustomerOrigin ADD TimestampCounter ROWVERSION
GO
