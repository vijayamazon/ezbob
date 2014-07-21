SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveExperianLtd') IS NOT NULL
	DROP PROCEDURE SaveExperianLtd
GO

IF TYPE_ID('ExperianLtdList') IS NOT NULL
	DROP TYPE ExperianLtdList
GO

CREATE TYPE ExperianLtdList AS TABLE (
	ServiceLogID BIGINT NOT NULL,
	RegisteredNumber NVARCHAR(255) NULL,
	LegalStatus NVARCHAR(255) NULL,
	IncorporationDate DATETIME NULL,
	DissolutionDate DATETIME NULL,
	CompanyName NVARCHAR(255) NULL,
	OfficeAddress1 NVARCHAR(255) NULL,
	OfficeAddress2 NVARCHAR(255) NULL,
	OfficeAddress3 NVARCHAR(255) NULL,
	OfficeAddress4 NVARCHAR(255) NULL,
	OfficeAddressPostcode NVARCHAR(255) NULL,
	CommercialDelphiScore INT NULL,
	StabilityOdds NVARCHAR(255) NULL,
	CommercialDelphiBandText NVARCHAR(255) NULL,
	CommercialDelphiCreditLimit DECIMAL(18, 6) NULL,
	SameTradingAddressG NVARCHAR(255) NULL,
	LengthOf1992SICArea INT NULL,
	TradingPhoneNumber NVARCHAR(255) NULL,
	PrincipalActivities NVARCHAR(255) NULL,
	First1992SICCodeDescription NVARCHAR(255) NULL,
	BankSortcode NVARCHAR(255) NULL,
	BankName NVARCHAR(255) NULL,
	BankAddress1 NVARCHAR(255) NULL,
	BankAddress2 NVARCHAR(255) NULL,
	BankAddress3 NVARCHAR(255) NULL,
	BankAddress4 NVARCHAR(255) NULL,
	BankAddressPostcode NVARCHAR(255) NULL,
	RegisteredNumberOfTheCurrentUltimateParentCompany NVARCHAR(255) NULL,
	RegisteredNameOfTheCurrentUltimateParentCompany NVARCHAR(255) NULL,
	TotalNumberOfCurrentDirectors INT NULL,
	NumberOfCurrentDirectorshipsLessThan12Months INT NULL,
	NumberOfAppointmentsInTheLast12Months INT NULL,
	NumberOfResignationsInTheLast12Months INT NULL,
	AgeOfMostRecentCCJDecreeMonths INT NULL,
	NumberOfCCJsDuringLast12Months INT NULL,
	ValueOfCCJsDuringLast12Months DECIMAL(18, 6) NULL,
	NumberOfCCJsBetween13And24MonthsAgo INT NULL,
	ValueOfCCJsBetween13And24MonthsAgo DECIMAL(18, 6) NULL,
	CompanyAverageDBT3Months DECIMAL(18, 6) NULL,
	CompanyAverageDBT6Months DECIMAL(18, 6) NULL,
	CompanyAverageDBT12Months DECIMAL(18, 6) NULL,
	CompanyNumberOfDbt1000 DECIMAL(18, 6) NULL,
	CompanyNumberOfDbt10000 DECIMAL(18, 6) NULL,
	CompanyNumberOfDbt100000 DECIMAL(18, 6) NULL,
	CompanyNumberOfDbt100000Plus DECIMAL(18, 6) NULL,
	IndustryAverageDBT3Months DECIMAL(18, 6) NULL,
	IndustryAverageDBT6Months DECIMAL(18, 6) NULL,
	IndustryAverageDBT12Months DECIMAL(18, 6) NULL,
	IndustryNumberOfDbt1000 DECIMAL(18, 6) NULL,
	IndustryNumberOfDbt10000 DECIMAL(18, 6) NULL,
	IndustryNumberOfDbt100000 DECIMAL(18, 6) NULL,
	IndustryNumberOfDbt100000Plus DECIMAL(18, 6) NULL,
	CompanyPaymentPattern NVARCHAR(255) NULL,
	IndustryPaymentPattern NVARCHAR(255) NULL,
	SupplierPaymentPattern NVARCHAR(255) NULL
)
GO

CREATE PROCEDURE SaveExperianLtd
@Tbl ExperianLtdList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ServiceLogID BIGINT
	DECLARE @ExperianLtdID BIGINT
	DECLARE @RegNum NVARCHAR(50)
	DECLARE @ParentRegNum NVARCHAR(15)
	DECLARE @c INT

	SELECT @c = COUNT(*) FROM @Tbl

	------------------------------------------------------------------------------

	IF @c != 1
		RAISERROR('Invalid argument: no/too much data to insert into ExperianLtd table.', 11, 1)

	------------------------------------------------------------------------------

	SELECT TOP 1
		@ServiceLogID = ServiceLogID,
		@RegNum = SUBSTRING(RegisteredNumber, 1, 50),
		@ParentRegNum = SUBSTRING(ISNULL(LTRIM(RTRIM(ISNULL(RegisteredNumberOfTheCurrentUltimateParentCompany, ''))), ''), 1, 15)
	FROM
		@Tbl

	EXECUTE DeleteParsedExperianLtd @ServiceLogID

	------------------------------------------------------------------------------

	INSERT INTO ExperianLtd (
		ServiceLogID,
		RegisteredNumber,
		LegalStatus,
		IncorporationDate,
		DissolutionDate,
		CompanyName,
		OfficeAddress1,
		OfficeAddress2,
		OfficeAddress3,
		OfficeAddress4,
		OfficeAddressPostcode,
		CommercialDelphiScore,
		StabilityOdds,
		CommercialDelphiBandText,
		CommercialDelphiCreditLimit,
		SameTradingAddressG,
		LengthOf1992SICArea,
		TradingPhoneNumber,
		PrincipalActivities,
		First1992SICCodeDescription,
		BankSortcode,
		BankName,
		BankAddress1,
		BankAddress2,
		BankAddress3,
		BankAddress4,
		BankAddressPostcode,
		RegisteredNumberOfTheCurrentUltimateParentCompany,
		RegisteredNameOfTheCurrentUltimateParentCompany,
		TotalNumberOfCurrentDirectors,
		NumberOfCurrentDirectorshipsLessThan12Months,
		NumberOfAppointmentsInTheLast12Months,
		NumberOfResignationsInTheLast12Months,
		AgeOfMostRecentCCJDecreeMonths,
		NumberOfCCJsDuringLast12Months,
		ValueOfCCJsDuringLast12Months,
		NumberOfCCJsBetween13And24MonthsAgo,
		ValueOfCCJsBetween13And24MonthsAgo,
		CompanyAverageDBT3Months,
		CompanyAverageDBT6Months,
		CompanyAverageDBT12Months,
		CompanyNumberOfDbt1000,
		CompanyNumberOfDbt10000,
		CompanyNumberOfDbt100000,
		CompanyNumberOfDbt100000Plus,
		IndustryAverageDBT3Months,
		IndustryAverageDBT6Months,
		IndustryAverageDBT12Months,
		IndustryNumberOfDbt1000,
		IndustryNumberOfDbt10000,
		IndustryNumberOfDbt100000,
		IndustryNumberOfDbt100000Plus,
		CompanyPaymentPattern,
		IndustryPaymentPattern,
		SupplierPaymentPattern
	) SELECT
		ServiceLogID,
		RegisteredNumber,
		LegalStatus,
		IncorporationDate,
		DissolutionDate,
		CompanyName,
		OfficeAddress1,
		OfficeAddress2,
		OfficeAddress3,
		OfficeAddress4,
		OfficeAddressPostcode,
		CommercialDelphiScore,
		StabilityOdds,
		CommercialDelphiBandText,
		CommercialDelphiCreditLimit,
		SameTradingAddressG,
		LengthOf1992SICArea,
		TradingPhoneNumber,
		PrincipalActivities,
		First1992SICCodeDescription,
		BankSortcode,
		BankName,
		BankAddress1,
		BankAddress2,
		BankAddress3,
		BankAddress4,
		BankAddressPostcode,
		RegisteredNumberOfTheCurrentUltimateParentCompany,
		RegisteredNameOfTheCurrentUltimateParentCompany,
		TotalNumberOfCurrentDirectors,
		NumberOfCurrentDirectorshipsLessThan12Months,
		NumberOfAppointmentsInTheLast12Months,
		NumberOfResignationsInTheLast12Months,
		AgeOfMostRecentCCJDecreeMonths,
		NumberOfCCJsDuringLast12Months,
		ValueOfCCJsDuringLast12Months,
		NumberOfCCJsBetween13And24MonthsAgo,
		ValueOfCCJsBetween13And24MonthsAgo,
		CompanyAverageDBT3Months,
		CompanyAverageDBT6Months,
		CompanyAverageDBT12Months,
		CompanyNumberOfDbt1000,
		CompanyNumberOfDbt10000,
		CompanyNumberOfDbt100000,
		CompanyNumberOfDbt100000Plus,
		IndustryAverageDBT3Months,
		IndustryAverageDBT6Months,
		IndustryAverageDBT12Months,
		IndustryNumberOfDbt1000,
		IndustryNumberOfDbt10000,
		IndustryNumberOfDbt100000,
		IndustryNumberOfDbt100000Plus,
		CompanyPaymentPattern,
		IndustryPaymentPattern,
		SupplierPaymentPattern
	FROM @Tbl

	------------------------------------------------------------------------------

	SET @ExperianLtdID = SCOPE_IDENTITY()

	------------------------------------------------------------------------------

	UPDATE MP_ServiceLog SET
		CompanyRefNum = @RegNum
	WHERE
		Id = @ServiceLogID

	------------------------------------------------------------------------------

	IF @ParentRegNum IS NOT NULL AND @ParentRegNum != ''
	BEGIN
		INSERT INTO MP_ExperianParentCompanyMap(ExperianRefNum, ExperianParentRefNum)
			VALUES(SUBSTRING(@RegNum, 1, 15), @ParentRegNum)
	END

	------------------------------------------------------------------------------

	SELECT @ExperianLtdID AS ExperianLtdID
END
GO
