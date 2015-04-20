SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCreditSafeTradingAddresses') IS NOT NULL
	DROP PROCEDURE SaveCreditSafeTradingAddresses
GO

IF TYPE_ID('CreditSafeTradingAddressesList') IS NOT NULL
	DROP TYPE CreditSafeTradingAddressesList
GO

CREATE TYPE CreditSafeTradingAddressesList AS TABLE (
	CreditSafeBaseDataID BIGINT NULL,
	Address1 NVARCHAR(100) NULL,
	Address2 NVARCHAR(100) NULL,
	Address3 NVARCHAR(100) NULL,
	Address4 NVARCHAR(100) NULL,
	PostCode NVARCHAR(10) NULL,
	Telephone NVARCHAR(20) NULL,
	TpsRegistered BIT NULL
)
GO

CREATE PROCEDURE SaveCreditSafeTradingAddresses
@Tbl CreditSafeTradingAddressesList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CreditSafeTradingAddresses (
		CreditSafeBaseDataID,
		Address1,
		Address2,
		Address3,
		Address4,
		PostCode,
		Telephone,
		TpsRegistered
	) SELECT
		CreditSafeBaseDataID,
		Address1,
		Address2,
		Address3,
		Address4,
		PostCode,
		Telephone,
		TpsRegistered
	FROM @Tbl
END
GO


