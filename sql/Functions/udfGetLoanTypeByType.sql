IF OBJECT_ID('dbo.udfGetLoanTypeByType') IS NOT NULL 
	DROP FUNCTION dbo.udfGetLoanTypeByType
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- Returns first loan type by that matches requested type (Type LIKE 'requested%').
-- If could not find loan type by type returns
-- any of loan types which are marked as default. If none is marked as
-- default returns loan type named 'StandardLoanType'.

CREATE FUNCTION dbo.udfGetLoanTypeByType(@LoanType NVARCHAR(50))
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
	DECLARE @Mask NVARCHAR(51)

	SET @Mask = LTRIM(RTRIM(ISNULL(@LoanType, '')))

	SET @Mask = CASE WHEN @Mask = '' THEN NULL ELSE @Mask + '%' END

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
			@Mask IS NOT NULL AND (
				lt.Type LIKE @Mask
				OR
				lt.IsDefault = 1
				OR
				lt.Type = 'StandardLoanType'
			)
		)
		OR (
			@Mask IS NULL AND (
				lt.IsDefault = 1
				OR
				lt.Type = 'StandardLoanType'
			)
		)
	ORDER BY
		CASE WHEN @Mask IS NOT NULL
			THEN CASE WHEN lt.Type LIKE @Mask THEN 0 ELSE 1 END
			ELSE 1
		END,		
		lt.IsDefault DESC

	RETURN
END
GO
