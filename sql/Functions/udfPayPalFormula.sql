IF OBJECT_ID('dbo.udfPayPalFormula') IS NOT NULL
	DROP FUNCTION dbo.udfPayPalFormula
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION dbo.udfPayPalFormula(
@order_items PayPalOrderItems READONLY,
@TheMonth DATETIME,
@NextMonth DATETIME,
@ForceIsSum BIT,
@FormulaName NVARCHAR(64)
)
RETURNS DECIMAL(18, 2)
AS
BEGIN
	DECLARE @FormulaID INT = NULL
	DECLARE @IsSum BIT = NULL
	DECLARE @Res DECIMAL(18, 2) = 0

	SELECT
		@FormulaID = FormulaID,
		@IsSum = IsSum
	FROM
		PayPalTotalsFormulae
	WHERE
		FormulaName = @FormulaName

	IF @FormulaID IS NULL
		RETURN @Res

	IF @ForceIsSum IS NOT NULL
		SET @IsSum = @ForceIsSum

	IF @IsSum = 1
	BEGIN
		SELECT
			@Res = SUM(ISNULL(i.NetAmount, 0))
		FROM
			@order_items i
			INNER JOIN PayPalTotalsFormulaTerms t
				ON t.FormulaID = @FormulaID
				AND i.[Type] = t.TransactionType
				AND i.[Status] = t.TransactionStatus
				AND 1 = (CASE
					WHEN t.TakePositive IS NULL THEN 1
					WHEN t.TakePositive = 1 THEN (CASE WHEN i.NetAmount >= 0 THEN 1 ELSE 0 END)
					ELSE                         (CASE WHEN i.NetAmount <= 0 THEN 1 ELSE 0 END)
				END)
		WHERE
			i.Created BETWEEN @TheMonth AND @NextMonth
	END
	ELSE BEGIN
		SELECT
			@Res = COUNT(*)
		FROM
			@order_items i
			INNER JOIN PayPalTotalsFormulaTerms t
				ON t.FormulaID = @FormulaID
				AND i.[Type] = t.TransactionType
				AND i.[Status] = t.TransactionStatus
				AND 1 = (CASE
					WHEN t.TakePositive IS NULL THEN 1
					WHEN t.TakePositive = 1 THEN (CASE WHEN i.NetAmount >= 0 THEN 1 ELSE 0 END)
					ELSE                         (CASE WHEN i.NetAmount <= 0 THEN 1 ELSE 0 END)
				END)
		WHERE
			i.Created BETWEEN @TheMonth AND @NextMonth
	END

	RETURN ISNULL(@Res, 0)
END
GO
