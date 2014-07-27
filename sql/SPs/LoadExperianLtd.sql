SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadExperianLtd') IS NULL
	EXECUTE('CREATE PROCEDURE LoadExperianLtd AS SELECT 1')
GO

ALTER PROCEDURE LoadExperianLtd
@ServiceLogID BIGINT,
@ExperianLtdID BIGINT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		@ExperianLtdID = ExperianLtdID
	FROM
		ExperianLtd
	WHERE
		ServiceLogID = @ServiceLogID

	SELECT
		'ExperianLtd' AS DatumType,
		ExperianLtdID AS ID,
		ServiceLogID AS ParentID,
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
		First1992SICCode,
		First1980SICCodeDescription,
		First1980SICCode,
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
	FROM
		ExperianLtd
	WHERE
		ExperianLtdID = @ExperianLtdID
END
GO
