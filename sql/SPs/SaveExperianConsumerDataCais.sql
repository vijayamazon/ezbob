SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveExperianConsumerDataCais') IS NOT NULL
	DROP PROCEDURE SaveExperianConsumerDataCais
GO

IF TYPE_ID('ExperianConsumerDataCaisList') IS NOT NULL
	DROP TYPE ExperianConsumerDataCaisList
GO

CREATE TYPE ExperianConsumerDataCaisList AS TABLE (
	ExperianConsumerDataId BIGINT NULL,
	CAISAccStartDate DATETIME NULL,
	SettlementDate DATETIME NULL,
	LastUpdatedDate DATETIME NULL,
	MatchTo INT NULL,
	CreditLimit INT NULL,
	Balance INT NULL,
	CurrentDefBalance INT NULL,
	DelinquentBalance INT NULL,
	AccountStatusCodes NVARCHAR(255) NULL,
	Status1To2 NVARCHAR(255) NULL,
	StatusTo3 NVARCHAR(255) NULL,
	NumOfMonthsHistory INT NULL,
	WorstStatus NVARCHAR(255) NULL,
	AccountStatus NVARCHAR(255) NULL,
	AccountType NVARCHAR(255) NULL,
	CompanyType NVARCHAR(255) NULL,
	RepaymentPeriod INT NULL,
	Payment INT NULL,
	NumAccountBalances INT NULL,
	NumCardHistories INT NULL
)
GO


CREATE PROCEDURE SaveExperianConsumerDataCais
@Tbl ExperianConsumerDataCaisList READONLY
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @ExperianConsumerDataCaisId BIGINT
	DECLARE @c INT

	SELECT @c = COUNT(*) FROM @Tbl

	IF @c = 0
		RAISERROR('Invalid argument: no/too much data to insert into SaveExperianConsumerDataCais table.', 11, 1)
		
	INSERT INTO ExperianConsumerDataCais (
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
	) SELECT
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
	FROM @Tbl
	
	SET @ExperianConsumerDataCaisId = SCOPE_IDENTITY()

	SELECT @ExperianConsumerDataCaisId AS ExperianConsumerDataCaisId
	
END
GO
