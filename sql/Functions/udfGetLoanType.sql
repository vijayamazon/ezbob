IF OBJECT_ID('dbo.udfGetLoanType') IS NOT NULL 
	DROP FUNCTION dbo.udfGetLoanType
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- Returns first loan type by id. If could not find loan type by id returns
-- any of loan types which are marked as default. If none is marked as
-- default returns loan type named 'StandardLoanType'.

CREATE FUNCTION dbo.udfGetLoanType(@LoanTypeID INT)
RETURNS @output TABLE (
	LoanTypeID INT,
	LoanType NVARCHAR(50),
	LoanTypeName NVARCHAR(250),
	Description NVARCHAR(MAX),
	IsDefault BIT,
	RepaymentPeriod INT
)
AS
BEGIN
	INSERT INTO @output(
		LoanTypeID,
		LoanType,
		LoanTypeName,
		Description,
		IsDefault,
		RepaymentPeriod
	)
	SELECT TOP 1
		lt.Id,
		lt.Type,
		lt.Name,
		lt.Description,
		lt.IsDefault,
		lt.RepaymentPeriod
	FROM
		LoanType lt
	WHERE (
			@LoanTypeID IS NOT NULL AND (
				lt.Id = @LoanTypeID
				OR
				lt.IsDefault = 1
				OR
				lt.Type = 'StandardLoanType'
			)
		)
		OR (
			@LoanTypeID IS NULL AND (
				lt.IsDefault = 1
				OR
				lt.Type = 'StandardLoanType'
			)
		)
	ORDER BY
		CASE WHEN @LoanTypeID IS NOT NULL
			THEN CASE WHEN lt.Id = @LoanTypeID THEN 0 ELSE 1 END
			ELSE 1
		END,		
		lt.IsDefault DESC

	RETURN
END
GO
