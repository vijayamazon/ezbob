IF OBJECT_ID('dbo.udfGetLoanSource') IS NOT NULL 
	DROP FUNCTION dbo.udfGetLoanSource
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- Returns loan source by id. If could not find loan source by id returns
-- any of loan sources which are marked as default. If none is marked as
-- default returns loan source with id = 1.

CREATE FUNCTION dbo.udfGetLoanSource(@LoanSourceID INT)
RETURNS @output TABLE (
	LoanSourceID INT,
	LoanSourceName NVARCHAR(50),
	MaxInterest DECIMAL(18, 6),
	DefaultRepaymentPeriod INT,
	IsCustomerRepaymentPeriodSelectionAllowed BIT,
	MaxEmployeeCount INT,
	MaxAnnualTurnover DECIMAL(18, 2),
	IsDefault BIT,
	AlertOnCustomerReasonType INT,
	IsDisabled BIT
)
AS
BEGIN
	INSERT INTO @output(
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
	)
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

	RETURN
END
GO
