SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCreditSafeNonLtdBaseDataFax') IS NOT NULL
	DROP PROCEDURE SaveCreditSafeNonLtdBaseDataFax
GO

IF TYPE_ID('CreditSafeNonLtdBaseDataFaxList') IS NOT NULL
	DROP TYPE CreditSafeNonLtdBaseDataFaxList
GO

CREATE TYPE CreditSafeNonLtdBaseDataFaxList AS TABLE (
	CreditSafeNonLtdBaseDataID BIGINT NULL,
	Fax NVARCHAR(20) NULL,
	FpsRegistered BIT NULL,
	Main BIT NULL
)
GO

CREATE PROCEDURE SaveCreditSafeNonLtdBaseDataFax
@Tbl CreditSafeNonLtdBaseDataFaxList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CreditSafeNonLtdBaseDataFax (
		CreditSafeNonLtdBaseDataID,
		Fax,
		FpsRegistered,
		Main
	) SELECT
		CreditSafeNonLtdBaseDataID,
		Fax,
		FpsRegistered,
		Main
	FROM @Tbl
END
GO


