SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCreditSafeNonLtdBaseDataTel') IS NOT NULL
	DROP PROCEDURE SaveCreditSafeNonLtdBaseDataTel
GO

IF TYPE_ID('CreditSafeNonLtdBaseDataTelList') IS NOT NULL
	DROP TYPE CreditSafeNonLtdBaseDataTelList
GO

CREATE TYPE CreditSafeNonLtdBaseDataTelList AS TABLE (
	CreditSafeNonLtdBaseDataID BIGINT NULL,
	Telephone NVARCHAR(20) NULL,
	TpsRegistered BIT NULL,
	Main BIT NULL
)
GO

CREATE PROCEDURE SaveCreditSafeNonLtdBaseDataTel
@Tbl CreditSafeNonLtdBaseDataTelList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CreditSafeNonLtdBaseDataTel (
		CreditSafeNonLtdBaseDataID,
		Telephone,
		TpsRegistered,
		Main
	) SELECT
		CreditSafeNonLtdBaseDataID,
		Telephone,
		TpsRegistered,
		Main
	FROM @Tbl
END
GO


