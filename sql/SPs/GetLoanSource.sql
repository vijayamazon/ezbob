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

	SELECT TOP 1
		ls.LoanSourceID,
		ls.LoanSourceName,
		ls.MaxInterest,
		ls.DefaultRepaymentPeriod,
		ls.IsCustomerRepaymentPeriodSelectionAllowed,
		ls.MaxEmployeeCount,
		ls.MaxAnnualTurnover,
		ls.IsDefault,
		ls.AlertOnCustomerReasonType,
		ls.IsDisabled
	FROM
		LoanSource ls
	WHERE (
			@LoanSourceID IS NOT NULL AND (
				ls.LoanSourceID = @LoanSourceID
				OR
				ls.IsDefault = 1
				OR
				ls.LoanSourceID = 1
			)
		)
		OR (
			@LoanSourceID IS NULL AND (
				ls.IsDefault = 1
				OR
				ls.LoanSourceID = 1
			)
		)
	ORDER BY
		CASE WHEN @LoanSourceID IS NOT NULL
			THEN CASE WHEN @LoanSourceID = LoanSourceID THEN 0 ELSE 1 END
			ELSE 1
		END,		
		ls.IsDefault DESC
END
GO
