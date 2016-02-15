SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('CompaniesHouseOfficerOrder') IS NULL
BEGIN
	CREATE TABLE [CompaniesHouseOfficerOrder] (
		[CompaniesHouseOfficerOrderID] INT IDENTITY(1, 1) NOT NULL,
		[CompanyRefNum] NVARCHAR(255) NULL,
		[Timestamp] DATETIME NOT NULL,
		[ActiveCount] INT NOT NULL,
		[Etag] NVARCHAR(255) NULL,
		[ItemsPerPage] INT NOT NULL,
		[Kind] NVARCHAR(255) NULL,
		[Link] NVARCHAR(255) NULL,
		[ResignedCount] INT NOT NULL,
		[StartIndex] INT NOT NULL,
		[TotalResults] INT NOT NULL,
		[Error] NVARCHAR(255) NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CompaniesHouseOfficerOrder PRIMARY KEY ([CompaniesHouseOfficerOrderID])
	)
END
GO

IF OBJECT_ID('CompaniesHouseOfficerOrderItem') IS NULL
BEGIN
	CREATE TABLE [CompaniesHouseOfficerOrderItem] (
		[CompaniesHouseOfficerOrderItemID] INT IDENTITY(1, 1) NOT NULL,
		[CompaniesHouseOfficerOrderID] INT NOT NULL,
		[AddressLine1] NVARCHAR(255) NULL,
		[AddressLine2] NVARCHAR(255) NULL,
		[CareOf] NVARCHAR(255) NULL,
		[Country] NVARCHAR(255) NULL,
		[Locality] NVARCHAR(255) NULL,
		[PoBox] NVARCHAR(255) NULL,
		[Postcode] NVARCHAR(255) NULL,
		[Premises] NVARCHAR(255) NULL,
		[Region] NVARCHAR(255) NULL,
		[AppointedOn] DATETIME NOT NULL,
		[CountryOfResidence] NVARCHAR(255) NULL,
		[DobDay] INT NULL,
		[DobMonth] INT NULL,
		[DobYear] INT NULL,
		[Link] NVARCHAR(255) NULL,
		[Name] NVARCHAR(255) NULL,
		[Nationality] NVARCHAR(255) NULL,
		[Occupation] NVARCHAR(255) NULL,
		[OfficerRole] NVARCHAR(255) NULL,
		[ResignedOn] DATETIME NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CompaniesHouseOfficerOrderItem PRIMARY KEY ([CompaniesHouseOfficerOrderItemID]),
		CONSTRAINT FK_CompaniesHouseOfficerOrderItem_CompaniesHouseOfficerOrderID FOREIGN KEY ([CompaniesHouseOfficerOrderID]) REFERENCES [CompaniesHouseOfficerOrder] ([CompaniesHouseOfficerOrderID])
	)
END
GO

IF OBJECT_ID('CompaniesHouseOfficerAppointmentOrder') IS NULL
BEGIN
	CREATE TABLE [CompaniesHouseOfficerAppointmentOrder] (
		[CompaniesHouseOfficerAppointmentOrderID] INT IDENTITY(1, 1) NOT NULL,
		[CompaniesHouseOfficerOrderItemID] INT NOT NULL,
		[DobDay] INT NULL,
		[DobMonth] INT NULL,
		[DobYear] INT NULL,
		[Etag] NVARCHAR(255) NULL,
		[IsCorporateOfficer] BIT NOT NULL,
		[ItemsPerPage] INT NOT NULL,
		[Kind] NVARCHAR(255) NULL,
		[Link] NVARCHAR(255) NULL,
		[Name] NVARCHAR(255) NULL,
		[StartIndex] INT NOT NULL,
		[TotalResults] INT NOT NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CompaniesHouseOfficerAppointmentOrder PRIMARY KEY ([CompaniesHouseOfficerAppointmentOrderID]),
		CONSTRAINT FK_CompaniesHouseOfficerAppointmentOrder_CompaniesHouseOfficerOrderItemID FOREIGN KEY ([CompaniesHouseOfficerOrderItemID]) REFERENCES [CompaniesHouseOfficerOrderItem] ([CompaniesHouseOfficerOrderItemID])
	)
END
GO

IF OBJECT_ID('CompaniesHouseOfficerAppointmentOrderItem') IS NULL
BEGIN
	CREATE TABLE [CompaniesHouseOfficerAppointmentOrderItem] (
		[CompaniesHouseOfficerAppointmetOrderItemID] INT IDENTITY(1, 1) NOT NULL,
		[CompaniesHouseOfficerAppointmentOrderID] INT NOT NULL,
		[AddressLine1] NVARCHAR(255) NULL,
		[AddressLine2] NVARCHAR(255) NULL,
		[CareOf] NVARCHAR(255) NULL,
		[Country] NVARCHAR(255) NULL,
		[Locality] NVARCHAR(255) NULL,
		[PoBox] NVARCHAR(255) NULL,
		[Postcode] NVARCHAR(255) NULL,
		[Premises] NVARCHAR(255) NULL,
		[Region] NVARCHAR(255) NULL,
		[AppointedBefore] DATETIME NULL,
		[AppointedOn] DATETIME NULL,
		[CompanyName] NVARCHAR(255) NULL,
		[CompanyNumber] NVARCHAR(255) NULL,
		[CompanyStatus] NVARCHAR(255) NULL,
		[CountryOfResidence] NVARCHAR(255) NULL,
		[IdentificationType] NVARCHAR(255) NULL,
		[LegalAuthority] NVARCHAR(255) NULL,
		[LegalForm] NVARCHAR(255) NULL,
		[PlaceRegistered] NVARCHAR(255) NULL,
		[RegistrationNumber] NVARCHAR(255) NULL,
		[IsPre1992Appointment] BIT NOT NULL,
		[Link] NVARCHAR(255) NULL,
		[Name] NVARCHAR(255) NULL,
		[Forename] NVARCHAR(255) NULL,
		[Honours] NVARCHAR(255) NULL,
		[OtherForenames] NVARCHAR(255) NULL,
		[Surname] NVARCHAR(255) NULL,
		[Title] NVARCHAR(255) NULL,
		[Nationality] NVARCHAR(255) NULL,
		[Occupation] NVARCHAR(255) NULL,
		[OfficerRole] NVARCHAR(255) NULL,
		[ResignedOn] DATETIME NULL,
		[TimestampCounter] ROWVERSION,
		CONSTRAINT PK_CompaniesHouseOfficerAppointmentOrderItem PRIMARY KEY ([CompaniesHouseOfficerAppointmetOrderItemID]),
		CONSTRAINT FK_CompaniesHouseOfficerAppointmentOrderItem_CompaniesHouseOfficerAppointmentOrderID FOREIGN KEY ([CompaniesHouseOfficerAppointmentOrderID]) REFERENCES [CompaniesHouseOfficerAppointmentOrder] ([CompaniesHouseOfficerAppointmentOrderID])
	)
END
GO

ALTER TABLE CompaniesHouseOfficerAppointmentOrderItem ALTER COLUMN AppointedOn DATETIME NULL
GO






