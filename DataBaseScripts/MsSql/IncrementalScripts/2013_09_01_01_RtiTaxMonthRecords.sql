IF OBJECT_ID('MP_RtiTaxMonthRecords') IS NULL
BEGIN
	CREATE TABLE MP_RtiTaxMonthRecords (
		Id INT IDENTITY(1, 1) NOT NULL,
		CustomerMarketPlaceId INT NOT NULL,
		Created DATETIME NOT NULL,
		CustomerMarketPlaceUpdatingHistoryRecordId INT NOT NULL,
		CONSTRAINT PK_RtiTaxMonthRecords PRIMARY KEY (Id),
		CONSTRAINT FK_RtiTaxMonthRecord_MP FOREIGN KEY (CustomerMarketPlaceId) REFERENCES MP_CustomerMarketPlace(Id)
	)

	CREATE TABLE MP_RtiTaxMonthEntries (
		Id INT IDENTITY(1, 1) NOT NULL,
		RecordId INT NOT NULL,
		DateStart DATETIME NOT NULL,
		DateEnd DATETIME NOT NULL,
		AmountPaid DECIMAL(18, 2) NOT NULL,
		AmountDue DECIMAL(18, 2) NOT NULL,
		CurrencyCode NVARCHAR(3) NOT NULL,
		CONSTRAINT PK_RtiTaxMonthEntries PRIMARY KEY (Id),
		CONSTRAINT FK_RtiTaxMonthEntries_Record FOREIGN KEY (RecordId) REFERENCES MP_RtiTaxMonthRecords (Id)
	)
END
GO
