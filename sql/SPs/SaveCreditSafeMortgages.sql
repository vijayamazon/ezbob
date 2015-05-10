SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCreditSafeMortgages') IS NOT NULL
	DROP PROCEDURE SaveCreditSafeMortgages
GO

IF TYPE_ID('CreditSafeMortgagesList') IS NOT NULL
	DROP TYPE CreditSafeMortgagesList
GO

CREATE TYPE CreditSafeMortgagesList AS TABLE (
	CreditSafeBaseDataID BIGINT NULL,
	MortgageType NVARCHAR(50) NULL,
	CreateDate DATETIME NULL,
	RegisterDate DATETIME NULL,
	SatisfiedDate DATETIME NULL,
	Status NVARCHAR(20) NULL,
	AmountSecured INT NULL,
	Details NVARCHAR(MAX) NULL
)
GO

CREATE PROCEDURE SaveCreditSafeMortgages
@Tbl CreditSafeMortgagesList READONLY
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @CreditSafeMortgagesID BIGINT

	INSERT INTO CreditSafeMortgages (
		CreditSafeBaseDataID,
		MortgageType,
		CreateDate,
		RegisterDate,
		SatisfiedDate,
		Status,
		AmountSecured,
		Details
	) SELECT
		CreditSafeBaseDataID,
		MortgageType,
		CreateDate,
		RegisterDate,
		SatisfiedDate,
		Status,
		AmountSecured,
		Details
	FROM @Tbl
	
	SET @CreditSafeMortgagesID = SCOPE_IDENTITY()

	SELECT @CreditSafeMortgagesID AS CreditSafeMortgagesID
END
GO


