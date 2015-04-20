SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCreditSafePreviousNames') IS NOT NULL
	DROP PROCEDURE SaveCreditSafePreviousNames
GO

IF TYPE_ID('CreditSafePreviousNamesList') IS NOT NULL
	DROP TYPE CreditSafePreviousNamesList
GO

CREATE TYPE CreditSafePreviousNamesList AS TABLE (
	CreditSafeBaseDataID BIGINT NULL,
	Name NVARCHAR(100) NULL,
	Date DATETIME NULL
)
GO

CREATE PROCEDURE SaveCreditSafePreviousNames
@Tbl CreditSafePreviousNamesList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CreditSafePreviousNames (
		CreditSafeBaseDataID,
		Name,
		Date
	) SELECT
		CreditSafeBaseDataID,
		Name,
		Date
	FROM @Tbl
END
GO


