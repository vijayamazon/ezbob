SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCreditSafeNonLtdMatchedCCJ') IS NOT NULL
	DROP PROCEDURE SaveCreditSafeNonLtdMatchedCCJ
GO

IF TYPE_ID('CreditSafeNonLtdMatchedCCJList') IS NOT NULL
	DROP TYPE CreditSafeNonLtdMatchedCCJList
GO

CREATE TYPE CreditSafeNonLtdMatchedCCJList AS TABLE (
	CreditSafeNonLtdBaseDataID BIGINT NULL,
	CaseNr NVARCHAR(10) NULL,
	CcjDate DATETIME NULL,
	CcjDatePaid DATETIME NULL,
	Court NVARCHAR(50) NULL,
	CcjStatus NVARCHAR(10) NULL,
	CcjAmount INT NULL,
	Against NVARCHAR(100) NULL,
	Address NVARCHAR(100) NULL
)
GO

CREATE PROCEDURE SaveCreditSafeNonLtdMatchedCCJ
@Tbl CreditSafeNonLtdMatchedCCJList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CreditSafeNonLtdMatchedCCJ (
		CreditSafeNonLtdBaseDataID,
		CaseNr,
		CcjDate,
		CcjDatePaid,
		Court,
		CcjStatus,
		CcjAmount,
		Against,
		Address
	) SELECT
		CreditSafeNonLtdBaseDataID,
		CaseNr,
		CcjDate,
		CcjDatePaid,
		Court,
		CcjStatus,
		CcjAmount,
		Against,
		Address
	FROM @Tbl
END
GO


