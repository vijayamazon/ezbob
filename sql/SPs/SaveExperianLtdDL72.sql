SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveExperianLtdDL72') IS NOT NULL
	DROP PROCEDURE SaveExperianLtdDL72
GO

IF TYPE_ID('ExperianLtdDL72List') IS NOT NULL
	DROP TYPE ExperianLtdDL72List
GO

CREATE TYPE ExperianLtdDL72List AS TABLE (
	ExperianLtdID BIGINT NOT NULL,
	ForeignAddressFlag NVARCHAR(255) NULL,
	IsCompany NVARCHAR(255) NULL,
	Number NVARCHAR(255) NULL,
	LengthOfDirectorship INT NULL,
	DirectorsAgeYears INT NULL,
	NumberOfConvictions INT NULL,
	Prefix NVARCHAR(255) NULL,
	FirstName NVARCHAR(255) NULL,
	MidName1 NVARCHAR(255) NULL,
	MidName2 NVARCHAR(255) NULL,
	LastName NVARCHAR(255) NULL,
	Suffix NVARCHAR(255) NULL,
	Qualifications NVARCHAR(255) NULL,
	Title NVARCHAR(255) NULL,
	CompanyName NVARCHAR(255) NULL,
	CompanyNumber NVARCHAR(255) NULL,
	ShareInfo NVARCHAR(255) NULL,
	BirthDate DATETIME NULL,
	HouseName NVARCHAR(255) NULL,
	HouseNumber NVARCHAR(255) NULL,
	Street NVARCHAR(255) NULL,
	Town NVARCHAR(255) NULL,
	County NVARCHAR(255) NULL,
	Postcode NVARCHAR(255) NULL
)
GO

CREATE PROCEDURE SaveExperianLtdDL72
@Tbl ExperianLtdDL72List READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO ExperianLtdDL72 (
		ExperianLtdID,
		ForeignAddressFlag,
		IsCompany,
		Number,
		LengthOfDirectorship,
		DirectorsAgeYears,
		NumberOfConvictions,
		Prefix,
		FirstName,
		MidName1,
		MidName2,
		LastName,
		Suffix,
		Qualifications,
		Title,
		CompanyName,
		CompanyNumber,
		ShareInfo,
		BirthDate,
		HouseName,
		HouseNumber,
		Street,
		Town,
		County,
		Postcode
	) SELECT
		ExperianLtdID,
		ForeignAddressFlag,
		IsCompany,
		Number,
		LengthOfDirectorship,
		DirectorsAgeYears,
		NumberOfConvictions,
		Prefix,
		FirstName,
		MidName1,
		MidName2,
		LastName,
		Suffix,
		Qualifications,
		Title,
		CompanyName,
		CompanyNumber,
		ShareInfo,
		BirthDate,
		HouseName,
		HouseNumber,
		Street,
		Town,
		County,
		Postcode
	FROM @Tbl
END
GO


