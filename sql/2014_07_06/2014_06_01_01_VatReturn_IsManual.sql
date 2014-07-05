SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('VatReturnRecordSources') IS NULL
BEGIN
	CREATE TABLE VatReturnRecordSources(
		SourceID INT NOT NULL,
		Source NVARCHAR(50) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT FK_VatReturnRecordSources PRIMARY KEY (SourceID),
		CONSTRAINT CHK_VatReturnRecordSources CHECK(LTRIM(RTRIM(Source)) != ''),
		CONSTRAINT UC_VatReturnRecordSources UNIQUE(Source)
	)

	INSERT INTO VatReturnRecordSources(SourceID, Source) VALUES
		(1, 'Linked'),
		(2, 'Uploaded'),
		(3, 'Manual')
END
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'SourceID' AND id = OBJECT_ID('MP_VatReturnRecords'))
BEGIN
	ALTER TABLE MP_VatReturnRecords ADD SourceID INT NOT NULL CONSTRAINT DF_VatReturnRecord_Source DEFAULT(1)

	ALTER TABLE MP_VatReturnRecords ADD CONSTRAINT FK_VatReturnRecord_Source FOREIGN KEY (SourceID) REFERENCES VatReturnRecordSources(SourceID)
END
GO

UPDATE MP_VatReturnRecords SET
	SourceID = 2
FROM
	MP_VatReturnRecords r
	INNER JOIN MP_CustomerMarketPlace m ON r.CustomerMarketPlaceID = m.Id
	INNER JOIN Customer c ON m.CustomerId = c.Id AND m.DisplayName = c.Name
GO


IF OBJECT_ID('DF_VatReturnRecord_Source') IS NOT NULL
	ALTER TABLE MP_VatReturnRecords DROP CONSTRAINT DF_VatReturnRecord_Source
GO

-- In the following two IF's default constraint is added and dropped because:
-- 1. it is necessary to add a NOT NULL column
-- 2. it is used to fill default value
-- 3. it is dropped because not needed any more

IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'SourceID' AND id = OBJECT_ID('MP_RtiTaxMonthRecords'))
BEGIN
	ALTER TABLE MP_RtiTaxMonthRecords ADD SourceID INT NOT NULL CONSTRAINT DF_RtiTaxMonthRecords_Source DEFAULT(1)

	ALTER TABLE MP_RtiTaxMonthRecords ADD CONSTRAINT FK_RtiTaxMonthRecords_Source FOREIGN KEY (SourceID) REFERENCES VatReturnRecordSources(SourceID)
END
GO

IF OBJECT_ID('DF_RtiTaxMonthRecords_Source') IS NOT NULL
	ALTER TABLE MP_RtiTaxMonthRecords DROP CONSTRAINT DF_RtiTaxMonthRecords_Source
GO
