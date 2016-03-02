SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadMinMaxLoanParams') IS NULL
	EXECUTE('CREATE PROCEDURE LoadMinMaxLoanParams AS SELECT 1')
GO

ALTER PROCEDURE LoadMinMaxLoanParams
AS
BEGIN
	;WITH minimax AS (
		SELECT DISTINCT
			OriginID,
			MinLoanAmount,
			MaxLoanAmount,
			MinTerm,
			MaxTerm
		FROM
			I_GradeRange
		WHERE
			IsActive = 1
	) SELECT
		OriginID,
		MinLoanAmount = MIN(MinLoanAmount),
		MaxLoanAmount = MAX(MaxLoanAmount),
		MinTerm = MIN(MinTerm),
		MaxTerm = MAX(MaxTerm)
	FROM
		minimax
	GROUP BY
		OriginID
END
GO
