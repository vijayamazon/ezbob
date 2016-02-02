IF OBJECT_ID('dbo.udfGetLoanSource') IS NOT NULL 
	DROP FUNCTION dbo.udfGetLoanSource
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- Returns loan source by id. If could not find loan source by id returns
-- default loan source for requested origin. If none is marked as
-- default returns loan source with the minimal existing loan source.

CREATE FUNCTION dbo.udfGetLoanSource(@LoanSourceID INT, @OriginID INT)
RETURNS @output TABLE (
	LoanSourceID INT,
	LoanSourceName NVARCHAR(50),
	MaxInterest DECIMAL(18, 6),
	DefaultRepaymentPeriod INT,
	IsCustomerRepaymentPeriodSelectionAllowed BIT,
	MaxEmployeeCount INT,
	MaxAnnualTurnover DECIMAL(18, 2),
	AlertOnCustomerReasonType INT,
	IsDisabled BIT
)
AS
BEGIN
	DECLARE @id INT = NULL

	IF @LoanSourceID IS NOT NULL
	BEGIN
		SELECT TOP 1
			@id = LoanSourceID
		FROM
			LoanSource
		WHERE
			LoanSourceID = @LoanSourceID
	END

	IF @id IS NULL
	BEGIN
		SELECT
			@id = LoanSourceID
		FROM
			DefaultLoanSources
		WHERE
			OriginID = @OriginID
	END

	IF @id IS NULL
	BEGIN
		SELECT
			@id = MIN(LoanSourceID)
		FROM
			LoanSource
	END

	INSERT INTO @output(
		LoanSourceID,
		LoanSourceName,
		MaxInterest,
		DefaultRepaymentPeriod,
		IsCustomerRepaymentPeriodSelectionAllowed,
		MaxEmployeeCount,
		MaxAnnualTurnover,
		AlertOnCustomerReasonType--,
	--	IsDisabled
	)
	SELECT TOP 1
		ls.LoanSourceID,
		ls.LoanSourceName,
		ls.MaxInterest,
		ls.DefaultRepaymentPeriod,
		ls.IsCustomerRepaymentPeriodSelectionAllowed,
		ls.MaxEmployeeCount,
		ls.MaxAnnualTurnover,
		ls.AlertOnCustomerReasonType--,
	--	ls.IsDisabled
	FROM
		LoanSource ls
	WHERE
		ls.LoanSourceID = @id

	RETURN
END
GO
