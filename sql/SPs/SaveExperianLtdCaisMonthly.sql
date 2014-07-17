SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveExperianLtdCaisMonthly') IS NOT NULL
	DROP PROCEDURE SaveExperianLtdCaisMonthly
GO

IF TYPE_ID('ExperianLtdCaisMonthlyList') IS NOT NULL
	DROP TYPE ExperianLtdCaisMonthlyList
GO

CREATE TYPE ExperianLtdCaisMonthlyList AS TABLE (
	ExperianLtdID BIGINT NOT NULL,
	NumberOfActiveAccounts DECIMAL(18, 6) NULL
)
GO

CREATE PROCEDURE SaveExperianLtdCaisMonthly
@Tbl ExperianLtdCaisMonthlyList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO ExperianLtdCaisMonthly (
		ExperianLtdID,
		NumberOfActiveAccounts
	) SELECT
		ExperianLtdID,
		NumberOfActiveAccounts
	FROM @Tbl
END
GO
