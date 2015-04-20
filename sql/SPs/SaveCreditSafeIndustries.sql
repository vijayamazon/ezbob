SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCreditSafeIndustries') IS NOT NULL
	DROP PROCEDURE SaveCreditSafeIndustries
GO

IF TYPE_ID('CreditSafeIndustriesList') IS NOT NULL
	DROP TYPE CreditSafeIndustriesList
GO

CREATE TYPE CreditSafeIndustriesList AS TABLE (
	CreditSafeBaseDataID BIGINT NULL,
	Name NVARCHAR(500) NULL
)
GO

CREATE PROCEDURE SaveCreditSafeIndustries
@Tbl CreditSafeIndustriesList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CreditSafeIndustries (
		CreditSafeBaseDataID,
		Name
	) SELECT
		CreditSafeBaseDataID,
		Name
	FROM @Tbl
END
GO


