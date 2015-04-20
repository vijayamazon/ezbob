SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCreditSafeBaseData_SecondarySicCodes') IS NOT NULL
	DROP PROCEDURE SaveCreditSafeBaseData_SecondarySicCodes
GO

IF TYPE_ID('CreditSafeBaseData_SecondarySicCodesList') IS NOT NULL
	DROP TYPE CreditSafeBaseData_SecondarySicCodesList
GO

CREATE TYPE CreditSafeBaseData_SecondarySicCodesList AS TABLE (
	CreditSafeBaseDataID BIGINT NULL,
	SicCode NVARCHAR(10) NULL,
	SicDescription NVARCHAR(500) NULL
)
GO

CREATE PROCEDURE SaveCreditSafeBaseData_SecondarySicCodes
@Tbl CreditSafeBaseData_SecondarySicCodesList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CreditSafeBaseData_SecondarySicCodes (
		CreditSafeBaseDataID,
		SicCode,
		SicDescription
	) SELECT
		CreditSafeBaseDataID,
		Siccode,
		SicDescription
	FROM @Tbl
END
GO


