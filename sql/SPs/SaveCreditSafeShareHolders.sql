SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCreditSafeShareHolders') IS NOT NULL
	DROP PROCEDURE SaveCreditSafeShareHolders
GO

IF TYPE_ID('CreditSafeShareHoldersList') IS NOT NULL
	DROP TYPE CreditSafeShareHoldersList
GO

CREATE TYPE CreditSafeShareHoldersList AS TABLE (
	CreditSafeBaseDataID BIGINT NULL,
	Name NVARCHAR(100) NULL,
	Shares NVARCHAR(250) NULL,
	Currency NVARCHAR(10) NULL
)
GO

CREATE PROCEDURE SaveCreditSafeShareHolders
@Tbl CreditSafeShareHoldersList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CreditSafeShareHolders (
		CreditSafeBaseDataID,
		Name,
		Shares,
		Currency
	) SELECT
		CreditSafeBaseDataID,
		Name,
		Shares,
		Currency
	FROM @Tbl
END
GO


