IF OBJECT_ID('GetLoanSource') IS NULL 
	EXECUTE('CREATE PROCEDURE GetLoanSource AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- Returns loan source by id. If could not find loan source by id returns
-- any of loan sources which are marked as default. If none is marked as
-- default returns loan source with id = 1.

ALTER PROCEDURE GetLoanSource
@LoanSourceID INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		LoanSourceID,
		LoanSourceName,
		MaxInterest,
		DefaultRepaymentPeriod,
		IsCustomerRepaymentPeriodSelectionAllowed,
		MaxEmployeeCount,
		MaxAnnualTurnover,
		IsDefault,
		AlertOnCustomerReasonType,
		IsDisabled
	FROM
		dbo.udfGetLoanSource(@LoanSourceID)
END
GO
