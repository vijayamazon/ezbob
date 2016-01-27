SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('CompaniesHouseOfficerAppointmentOrderItemSave') IS NOT NULL
	DROP PROCEDURE CompaniesHouseOfficerAppointmentOrderItemSave
GO

IF TYPE_ID('CompaniesHouseOfficerAppointmentOrderItemList') IS NOT NULL
	DROP TYPE CompaniesHouseOfficerAppointmentOrderItemList
GO

CREATE TYPE CompaniesHouseOfficerAppointmentOrderItemList AS TABLE (
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
	[ResignedOn] DATETIME NULL
)
GO

CREATE PROCEDURE CompaniesHouseOfficerAppointmentOrderItemSave
@Tbl CompaniesHouseOfficerAppointmentOrderItemList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CompaniesHouseOfficerAppointmentOrderItem (
		[CompaniesHouseOfficerAppointmentOrderID],
		[AddressLine1],
		[AddressLine2],
		[CareOf],
		[Country],
		[Locality],
		[PoBox],
		[Postcode],
		[Premises],
		[Region],
		[AppointedBefore],
		[AppointedOn],
		[CompanyName],
		[CompanyNumber],
		[CompanyStatus],
		[CountryOfResidence],
		[IdentificationType],
		[LegalAuthority],
		[LegalForm],
		[PlaceRegistered],
		[RegistrationNumber],
		[IsPre1992Appointment],
		[Link],
		[Name],
		[Forename],
		[Honours],
		[OtherForenames],
		[Surname],
		[Title],
		[Nationality],
		[Occupation],
		[OfficerRole],
		[ResignedOn]
	) SELECT
		[CompaniesHouseOfficerAppointmentOrderID],
		[AddressLine1],
		[AddressLine2],
		[CareOf],
		[Country],
		[Locality],
		[PoBox],
		[Postcode],
		[Premises],
		[Region],
		[AppointedBefore],
		[AppointedOn],
		[CompanyName],
		[CompanyNumber],
		[CompanyStatus],
		[CountryOfResidence],
		[IdentificationType],
		[LegalAuthority],
		[LegalForm],
		[PlaceRegistered],
		[RegistrationNumber],
		[IsPre1992Appointment],
		[Link],
		[Name],
		[Forename],
		[Honours],
		[OtherForenames],
		[Surname],
		[Title],
		[Nationality],
		[Occupation],
		[OfficerRole],
		[ResignedOn]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


