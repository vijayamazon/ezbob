SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadExperianConsumerDataCais') IS NULL
	EXECUTE('CREATE PROCEDURE LoadExperianConsumerDataCais AS SELECT 1')
GO

ALTER PROCEDURE LoadExperianConsumerDataCais
@ExperianConsumerDataId BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		'ExperianConsumerDataCais' AS DatumType,
		Id,
		ExperianConsumerDataId,
		CAISAccStartDate,
		SettlementDate,
		LastUpdatedDate,
		MatchTo,
		CreditLimit,
		Balance,
		CurrentDefBalance,
		DelinquentBalance,
		AccountStatusCodes,
		Status1To2,
		StatusTo3,
		NumOfMonthsHistory,
		WorstStatus,
		AccountStatus,
		AccountType,
		CompanyType,
		RepaymentPeriod,
		Payment,
		NumAccountBalances,
		NumCardHistories
	FROM
		ExperianConsumerDataCais r
	WHERE
		r.ExperianConsumerDataId = @ExperianConsumerDataId
END
GO

