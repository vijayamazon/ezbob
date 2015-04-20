SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCreditSafeCreditRatings') IS NOT NULL
	DROP PROCEDURE SaveCreditSafeCreditRatings
GO

IF TYPE_ID('CreditSafeCreditRatingsList') IS NOT NULL
	DROP TYPE CreditSafeCreditRatingsList
GO

CREATE TYPE CreditSafeCreditRatingsList AS TABLE (
	CreditSafeBaseDataID BIGINT NULL,
	Date DATETIME NULL,
	Score INT NULL,
	Description NVARCHAR(500) NULL
)
GO

CREATE PROCEDURE SaveCreditSafeCreditRatings
@Tbl CreditSafeCreditRatingsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CreditSafeCreditRatings (
		CreditSafeBaseDataID,
		Date,
		Score,
		Description
	) SELECT
		CreditSafeBaseDataID,
		Date,
		Score,
		Description
	FROM @Tbl
END
GO


