SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCreditSafeNonLtdPossibleCCJ') IS NOT NULL
	DROP PROCEDURE SaveCreditSafeNonLtdPossibleCCJ
GO

IF TYPE_ID('CreditSafeNonLtdPossibleCCJList') IS NOT NULL
	DROP TYPE CreditSafeNonLtdPossibleCCJList
GO

CREATE TYPE CreditSafeNonLtdPossibleCCJList AS TABLE (
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

CREATE PROCEDURE SaveCreditSafeNonLtdPossibleCCJ
@Tbl CreditSafeNonLtdPossibleCCJList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CreditSafeNonLtdPossibleCCJ (
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


