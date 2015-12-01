SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('PostcodeNutsSave') IS NOT NULL
	DROP PROCEDURE PostcodeNutsSave
GO

IF TYPE_ID('PostcodeNutsList') IS NOT NULL
	DROP TYPE PostcodeNutsList
GO

CREATE TYPE PostcodeNutsList AS TABLE (
	[Postcode] NVARCHAR(10) NULL,
	[NutsCode] NVARCHAR(200) NULL,
	[Nuts] NVARCHAR(200) NULL,
	[Quality] INT NOT NULL,
	[Eastings] BIGINT NOT NULL,
	[Northings] BIGINT NOT NULL,
	[Country] NVARCHAR(200) NULL,
	[Nhs_ha] NVARCHAR(200) NULL,
	[Longitude] DECIMAL(18, 6) NOT NULL,
	[Latitude] DECIMAL(18, 6) NOT NULL,
	[ParliamentaryConstituency] NVARCHAR(200) NULL,
	[EuropeanElectoralRegion] NVARCHAR(200) NULL,
	[PrimaryCareTrust] NVARCHAR(200) NULL,
	[Region] NVARCHAR(200) NULL,
	[Lsoa] NVARCHAR(200) NULL,
	[Msoa] NVARCHAR(200) NULL,
	[Incode] NVARCHAR(10) NULL,
	[Outcode] NVARCHAR(10) NULL,
	[AdminDistrict] NVARCHAR(200) NULL,
	[AdminDistrictCode] NVARCHAR(50) NULL,
	[Parish] NVARCHAR(200) NULL,
	[ParishCode] NVARCHAR(50) NULL,
	[AdminCounty] NVARCHAR(200) NULL,
	[AdminCountyCode] NVARCHAR(50) NULL,
	[AdminWard] NVARCHAR(200) NULL,
	[AdminWardCode] NVARCHAR(50) NULL,
	[Ccg] NVARCHAR(200) NULL,
	[CcgCode] NVARCHAR(50) NULL,
	[Status] INT NOT NULL
)
GO

CREATE PROCEDURE PostcodeNutsSave
@Tbl PostcodeNutsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO PostcodeNuts (
		[Postcode],
		[NutsCode],
		[Nuts],
		[Quality],
		[Eastings],
		[Northings],
		[Country],
		[Nhs_ha],
		[Longitude],
		[Latitude],
		[ParliamentaryConstituency],
		[EuropeanElectoralRegion],
		[PrimaryCareTrust],
		[Region],
		[Lsoa],
		[Msoa],
		[Incode],
		[Outcode],
		[AdminDistrict],
		[AdminDistrictCode],
		[Parish],
		[ParishCode],
		[AdminCounty],
		[AdminCountyCode],
		[AdminWard],
		[AdminWardCode],
		[Ccg],
		[CcgCode],
		[Status]
	) SELECT
		[Postcode],
		[NutsCode],
		[Nuts],
		[Quality],
		[Eastings],
		[Northings],
		[Country],
		[Nhs_ha],
		[Longitude],
		[Latitude],
		[ParliamentaryConstituency],
		[EuropeanElectoralRegion],
		[PrimaryCareTrust],
		[Region],
		[Lsoa],
		[Msoa],
		[Incode],
		[Outcode],
		[AdminDistrict],
		[AdminDistrictCode],
		[Parish],
		[ParishCode],
		[AdminCounty],
		[AdminCountyCode],
		[AdminWard],
		[AdminWardCode],
		[Ccg],
		[CcgCode],
		[Status]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO