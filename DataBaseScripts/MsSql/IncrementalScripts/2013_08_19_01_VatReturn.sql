IF OBJECT_ID('Business') IS NULL
BEGIN
	CREATE TABLE Business (
		Id INT IDENTITY(1, 1) NOT NULL,
		Name NVARCHAR(256) NOT NULL,
		Address NVARCHAR(4000) NOT NULL,
		CONSTRAINT PK_Business PRIMARY KEY (Id)
	)
	
	CREATE UNIQUE INDEX IDX_Business ON Business (Name, Address)
	
	CREATE TABLE MP_VatReturnRecords (
		Id INT IDENTITY(1, 1) NOT NULL,
		CustomerMarketPlaceId INT NOT NULL,
		Created DATETIME NOT NULL,
		CustomerMarketPlaceUpdatingHistoryRecordId INT NOT NULL,
		Period NVARCHAR(256) NOT NULL,
		DateFrom DATETIME NOT NULL,
		DateTo DATETIME NOT NULL,
		DateDue DATETIME NOT NULL,
		RegistrationNo NVARCHAR(256) NOT NULL,
		BusinessId INT NOT NULL,
		CONSTRAINT PK_VatReturnRecords PRIMARY KEY (Id),
		CONSTRAINT FK_VatReturn_MarketPlace FOREIGN KEY (CustomerMarketPlaceId) REFERENCES MP_CustomerMarketPlace (Id),
		CONSTRAINT FK_VatReturn_Business FOREIGN KEY (BusinessId) REFERENCES Business (Id)
	)
	
	CREATE TABLE MP_VatReturnEntryNames (
		Id INT IDENTITY(1, 1) NOT NULL,
		Name NVARCHAR(512) NOT NULL,
		CONSTRAINT PK_VatReturnEntryNames PRIMARY KEY (Id),
	)
	
	CREATE UNIQUE INDEX IDX_VatReturnEntryNames ON MP_VatReturnEntryNames (Name)
	
	CREATE TABLE MP_VatReturnEntries (
		Id INT IDENTITY(1, 1) NOT NULL,
		RecordId INT NOT NULL,
		NameId INT NOT NULL,
		Amount DECIMAL(18, 2) NOT NULL,
		CurrencyCode NVARCHAR(3) NOT NULL
	)
END
GO
