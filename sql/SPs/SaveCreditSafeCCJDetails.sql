SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCreditSafeCCJDetails') IS NOT NULL
	DROP PROCEDURE SaveCreditSafeCCJDetails
GO

IF TYPE_ID('CreditSafeCCJDetailsList') IS NOT NULL
	DROP TYPE CreditSafeCCJDetailsList
GO

CREATE TYPE CreditSafeCCJDetailsList AS TABLE (
	CreditSafeBaseDataID BIGINT NULL,
	CaseNr NVARCHAR(10) NULL,
	CcjDate DATETIME NULL,
	Court NVARCHAR(50) NULL,
	CcjDatePaid DATETIME NULL,
	CcjStatus NVARCHAR(10) NULL,
	CcjAmount INT NULL
)
GO

CREATE PROCEDURE SaveCreditSafeCCJDetails
@Tbl CreditSafeCCJDetailsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CreditSafeCCJDetails (
		CreditSafeBaseDataID,
		CaseNr,
		CcjDate,
		Court,
		CcjDatePaid,
		CcjStatus,
		CcjAmount
	) SELECT
		CreditSafeBaseDataID,
		CaseNr,
		CcjDate,
		Court,
		CcjDatePaid,
		CcjStatus,
		CcjAmount
	FROM @Tbl
END
GO


