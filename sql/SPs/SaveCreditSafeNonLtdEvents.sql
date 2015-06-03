SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCreditSafeNonLtdEvents') IS NOT NULL
	DROP PROCEDURE SaveCreditSafeNonLtdEvents
GO

IF TYPE_ID('CreditSafeNonLtdEventsList') IS NOT NULL
	DROP TYPE CreditSafeNonLtdEventsList
GO

CREATE TYPE CreditSafeNonLtdEventsList AS TABLE (
	CreditSafeNonLtdBaseDataID BIGINT NULL,
	Date DATETIME NULL,
	Text NVARCHAR(250) NULL
)
GO

CREATE PROCEDURE SaveCreditSafeNonLtdEvents
@Tbl CreditSafeNonLtdEventsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CreditSafeNonLtdEvents (
		CreditSafeNonLtdBaseDataID,
		Date,
		Text
	) SELECT
		CreditSafeNonLtdBaseDataID,
		Date,
		Text
	FROM @Tbl
END
GO


