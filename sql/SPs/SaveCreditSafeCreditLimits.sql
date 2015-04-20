SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCreditSafeCreditLimits') IS NOT NULL
	DROP PROCEDURE SaveCreditSafeCreditLimits
GO

IF TYPE_ID('CreditSafeCreditLimitsList') IS NOT NULL
	DROP TYPE CreditSafeCreditLimitsList
GO

CREATE TYPE CreditSafeCreditLimitsList AS TABLE (
	CreditSafeBaseDataID BIGINT NULL,
	Limit INT NULL,
	Date DATETIME NULL
)
GO

CREATE PROCEDURE SaveCreditSafeCreditLimits
@Tbl CreditSafeCreditLimitsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CreditSafeCreditLimits (
		CreditSafeBaseDataID,
		Limit,
		Date
	) SELECT
		CreditSafeBaseDataID,
		Limit,
		Date
	FROM @Tbl
END
GO


