SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('RptLoansForLsaAgreementsBasePaths') IS NULL
	EXECUTE('CREATE PROCEDURE RptLoansForLsaAgreementsBasePaths AS SELECT 1')
GO

ALTER PROCEDURE RptLoansForLsaAgreementsBasePaths
AS
BEGIN
	SELECT
		Value
	FROM
		ConfigurationVariables
	WHERE
		Name LIKE '%Agreement%'
END
GO
