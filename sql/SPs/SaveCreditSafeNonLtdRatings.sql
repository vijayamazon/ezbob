SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCreditSafeNonLtdRatings') IS NOT NULL
	DROP PROCEDURE SaveCreditSafeNonLtdRatings
GO

IF TYPE_ID('CreditSafeNonLtdRatingsList') IS NOT NULL
	DROP TYPE CreditSafeNonLtdRatingsList
GO

CREATE TYPE CreditSafeNonLtdRatingsList AS TABLE (
	CreditSafeNonLtdBaseDataID BIGINT NULL,
	Date DATETIME NULL,
	Score INT NULL,
	Description NVARCHAR(100) NULL
)
GO

CREATE PROCEDURE SaveCreditSafeNonLtdRatings
@Tbl CreditSafeNonLtdRatingsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CreditSafeNonLtdRatings (
		CreditSafeNonLtdBaseDataID,
		Date,
		Score,
		Description
	) SELECT
		CreditSafeNonLtdBaseDataID,
		Date,
		Score,
		Description
	FROM @Tbl
END
GO


