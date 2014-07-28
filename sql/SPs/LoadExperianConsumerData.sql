SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadExperianConsumerData') IS NULL
	EXECUTE('CREATE PROCEDURE LoadExperianConsumerData AS SELECT 1')
GO

ALTER PROCEDURE LoadExperianConsumerData
@ServiceLogId BIGINT,
@ExperianConsumerId BIGINT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		@ExperianConsumerId = Id
	FROM
		ExperianConsumerData
	WHERE
		ServiceLogId = @ServiceLogId

	SELECT
		'ExperianConsumerData' AS DatumType,
		Id,
		ServiceLogId,
		CustomerId,
		DirectorId,
		Error,
		BureauScore,
		CII,
		CreditCardBalances,
		ActiveCaisBalanceExcMortgages,
		NumCreditCards,
		CreditLimitUtilisation,
		CreditCardOverLimit,
		PersonalLoanStatus,
		WorstStatus,
		WorstCurrentStatus,
		WorstHistoricalStatus,
		TotalAccountBalances,
		NumAccounts,
		NumCCJs,
		CCJLast2Years,
		TotalCCJValue1,
		TotalCCJValue2,
		EnquiriesLast6Months,
		EnquiriesLast3Months,
		MortgageBalance,
		CaisDOB,
		CAISDefaults,
		BadDebt,
		CAISSpecialInstructionFlag
	FROM
		ExperianConsumerData
	WHERE
		Id = @ExperianConsumerId
END
GO
