SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadExperianLtdDL97') IS NULL
	EXECUTE('CREATE PROCEDURE LoadExperianLtdDL97 AS SELECT 1')
GO

ALTER PROCEDURE LoadExperianLtdDL97
@ExperianLtdID BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		'ExperianLtdDL97' AS DatumType,
		ExperianLtdDL97ID,
		ExperianLtdID,
		AccountState,
		CompanyType,
		AccountType,
		DefaultDate,
		SettlementDate,
		CurrentBalance,
		Status12,
		Status39,
		CAISLastUpdatedDate,
		AccountStatusLast12AccountStatuses,
		AgreementNumber,
		MonthsData,
		DefaultBalance
	FROM
		ExperianLtdDL97
	WHERE
		ExperianLtdID = @ExperianLtdID
END
GO

