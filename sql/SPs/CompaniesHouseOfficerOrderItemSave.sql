SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('CompaniesHouseOfficerOrderItemSave') IS NOT NULL
	DROP PROCEDURE CompaniesHouseOfficerOrderItemSave
GO

IF TYPE_ID('CompaniesHouseOfficerOrderItemList') IS NOT NULL
	DROP TYPE CompaniesHouseOfficerOrderItemList
GO

CREATE TYPE CompaniesHouseOfficerOrderItemList AS TABLE (
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
	[ResignedOn] DATETIME NULL
)
GO

CREATE PROCEDURE CompaniesHouseOfficerOrderItemSave
@Tbl CompaniesHouseOfficerOrderItemList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CompaniesHouseOfficerOrderItem (
		[CompaniesHouseOfficerOrderID],
		[AddressLine1],
		[AddressLine2],
		[CareOf],
		[Country],
		[Locality],
		[PoBox],
		[Postcode],
		[Premises],
		[Region],
		[AppointedOn],
		[CountryOfResidence],
		[DobDay],
		[DobMonth],
		[DobYear],
		[Link],
		[Name],
		[Nationality],
		[Occupation],
		[OfficerRole],
		[ResignedOn]
	) SELECT
		[CompaniesHouseOfficerOrderID],
		[AddressLine1],
		[AddressLine2],
		[CareOf],
		[Country],
		[Locality],
		[PoBox],
		[Postcode],
		[Premises],
		[Region],
		[AppointedOn],
		[CountryOfResidence],
		[DobDay],
		[DobMonth],
		[DobYear],
		[Link],
		[Name],
		[Nationality],
		[Occupation],
		[OfficerRole],
		[ResignedOn]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


