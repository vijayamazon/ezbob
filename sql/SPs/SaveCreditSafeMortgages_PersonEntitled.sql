SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCreditSafeMortgages_PersonEntitled') IS NOT NULL
	DROP PROCEDURE SaveCreditSafeMortgages_PersonEntitled
GO

IF TYPE_ID('CreditSafeMortgages_PersonEntitledList') IS NOT NULL
	DROP TYPE CreditSafeMortgages_PersonEntitledList
GO

CREATE TYPE CreditSafeMortgages_PersonEntitledList AS TABLE (
	CreditSafeMortgagesID BIGINT NULL,
	Name NVARCHAR(500) NULL
)
GO

CREATE PROCEDURE SaveCreditSafeMortgages_PersonEntitled
@Tbl CreditSafeMortgages_PersonEntitledList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CreditSafeMortgages_PersonEntitled (
		CreditSafeMortgagesID,
		Name
	) SELECT
		CreditSafeMortgagesID,
		Name
	FROM @Tbl
END
GO


