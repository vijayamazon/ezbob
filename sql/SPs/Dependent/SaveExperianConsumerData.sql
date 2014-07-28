SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveExperianConsumerData') IS NOT NULL
	DROP PROCEDURE SaveExperianConsumerData
GO

IF TYPE_ID('ExperianConsumerDataList') IS NOT NULL
	DROP TYPE ExperianConsumerDataList
GO

CREATE TYPE ExperianConsumerDataList AS TABLE (
	ServiceLogId BIGINT NULL,
	CustomerId INT NULL,
	DirectorId INT NULL,
	Error NVARCHAR(255) NULL,
	HasParsingError BIT NOT NULL,
	HasExperianError BIT NOT NULL,
	BureauScore INT NULL,
	CII INT NULL,
	CreditCardBalances INT NULL,
	ActiveCaisBalanceExcMortgages INT NULL,
	NumCreditCards INT NULL,
	CreditLimitUtilisation INT NULL,
	CreditCardOverLimit INT NULL,
	PersonalLoanStatus NVARCHAR(255) NULL,
	WorstStatus NVARCHAR(255) NULL,
	WorstCurrentStatus NVARCHAR(255) NULL,
	WorstHistoricalStatus NVARCHAR(255) NULL,
	TotalAccountBalances INT NULL,
	NumAccounts INT NULL,
	NumCCJs INT NULL,
	CCJLast2Years INT NULL,
	TotalCCJValue1 INT NULL,
	TotalCCJValue2 INT NULL,
	EnquiriesLast6Months INT NULL,
	EnquiriesLast3Months INT NULL,
	MortgageBalance INT NULL,
	CaisDOB DATETIME NULL,
	CreditCommitmentsRevolving INT NULL,
	CreditCommitmentsNonRevolving INT NULL,
	MortgagePayments INT NULL,
	Bankruptcy BIT NOT NULL,
	OtherBankruptcy BIT NOT NULL,
	CAISDefaults INT NULL,
	BadDebt NVARCHAR(255) NULL,
	NOCsOnCCJ BIT NOT NULL,
	NOCsOnCAIS BIT NOT NULL,
	NOCAndNOD BIT NOT NULL,
	SatisfiedJudgement BIT NOT NULL,
	CAISSpecialInstructionFlag NVARCHAR(255) NULL
)
GO

CREATE PROCEDURE SaveExperianConsumerData
@Tbl ExperianConsumerDataList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ExperianConsumerDataId BIGINT
	DECLARE @ServiceLogId BIGINT
	DECLARE @c INT

	SELECT @c = COUNT(*) FROM @Tbl

	IF @c != 1
		RAISERROR('Invalid argument: no/too much data to insert into ExperianConsumerData table.', 11, 1)
	
	SELECT TOP 1
		@ServiceLogId = ServiceLogId
	FROM
		@Tbl
	
	EXECUTE DeleteParsedExperianConsumer @ServiceLogId
	
	INSERT INTO ExperianConsumerData (
		ServiceLogId,
		CustomerId,
		DirectorId,
		Error,
		HasParsingError,
		HasExperianError,
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
		CreditCommitmentsRevolving,
		CreditCommitmentsNonRevolving,
		MortgagePayments,
		Bankruptcy,
		OtherBankruptcy,
		CAISDefaults,
		BadDebt,
		NOCsOnCCJ,
		NOCsOnCAIS,
		NOCAndNOD,
		SatisfiedJudgement,
		CAISSpecialInstructionFlag
	) SELECT
		ServiceLogId,
		CustomerId,
		DirectorId,
		Error,
		HasParsingError,
		HasExperianError,
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
		CreditCommitmentsRevolving,
		CreditCommitmentsNonRevolving,
		MortgagePayments,
		Bankruptcy,
		OtherBankruptcy,
		CAISDefaults,
		BadDebt,
		NOCsOnCCJ,
		NOCsOnCAIS,
		NOCAndNOD,
		SatisfiedJudgement,
		CAISSpecialInstructionFlag
	FROM @Tbl
	
	SET @ExperianConsumerDataId = SCOPE_IDENTITY()

	SELECT @ExperianConsumerDataId AS ExperianConsumerDataId

END
GO