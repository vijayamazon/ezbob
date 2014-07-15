SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveExperianLtdDL97') IS NOT NULL
	DROP PROCEDURE SaveExperianLtdDL97
GO

IF TYPE_ID('ExperianLtdDL97List') IS NOT NULL
	DROP TYPE ExperianLtdDL97List
GO

CREATE TYPE ExperianLtdDL97List AS TABLE (
	ExperianLtdID BIGINT NOT NULL,
	AccountState NVARCHAR(255) NULL,
	CompanyType INT NULL,
	AccountType INT NULL,
	DefaultDate DATETIME NULL,
	SettlementDate DATETIME NULL,
	CurrentBalance DECIMAL(18, 6) NULL,
	Status12 DECIMAL(18, 6) NULL,
	Status39 DECIMAL(18, 6) NULL,
	CAISLastUpdatedDate DATETIME NULL,
	AccountStatusLast12AccountStatuses NVARCHAR(255) NULL,
	AgreementNumber NVARCHAR(255) NULL,
	MonthsData NVARCHAR(255) NULL,
	DefaultBalance DECIMAL(18, 6) NULL
)
GO

CREATE PROCEDURE SaveExperianLtdDL97
@Tbl ExperianLtdDL97List READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO ExperianLtdDL97 (
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
	) SELECT
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
	FROM @Tbl
END
GO


