SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveExperianConsumerDataCaisBalance') IS NOT NULL
	DROP PROCEDURE SaveExperianConsumerDataCaisBalance
GO

IF TYPE_ID('ExperianConsumerDataCaisBalanceList') IS NOT NULL
	DROP TYPE ExperianConsumerDataCaisBalanceList
GO

CREATE TYPE ExperianConsumerDataCaisBalanceList AS TABLE (
	ExperianConsumerDataCaisId BIGINT NULL,
	AccountBalance INT NULL,
	Status NVARCHAR(255) NULL
)
GO

CREATE PROCEDURE SaveExperianConsumerDataCaisBalance
@Tbl ExperianConsumerDataCaisBalanceList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO ExperianConsumerDataCaisBalance (
		ExperianConsumerDataCaisId,
		AccountBalance,
		Status
	) SELECT
		ExperianConsumerDataCaisId,
		AccountBalance,
		Status
	FROM @Tbl
END
GO