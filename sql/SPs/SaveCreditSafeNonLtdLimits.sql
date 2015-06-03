SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCreditSafeNonLtdLimits') IS NOT NULL
	DROP PROCEDURE SaveCreditSafeNonLtdLimits
GO

IF TYPE_ID('CreditSafeNonLtdLimitsList') IS NOT NULL
	DROP TYPE CreditSafeNonLtdLimitsList
GO

CREATE TYPE CreditSafeNonLtdLimitsList AS TABLE (
	CreditSafeNonLtdBaseDataID BIGINT NULL,
	Limit INT NULL,
	Date DATETIME NULL
)
GO

CREATE PROCEDURE SaveCreditSafeNonLtdLimits
@Tbl CreditSafeNonLtdLimitsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CreditSafeNonLtdLimits (
		CreditSafeNonLtdBaseDataID,
		Limit,
		Date
	) SELECT
		CreditSafeNonLtdBaseDataID,
		Limit,
		Date
	FROM @Tbl
END
GO


