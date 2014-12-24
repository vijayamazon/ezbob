IF OBJECT_ID('dbo.udfYodleeFormula') IS NOT NULL
	DROP FUNCTION dbo.udfYodleeFormula
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION dbo.udfYodleeFormula(
@order_items YodleeOrderItems READONLY,
@TheMonth DATETIME,
@NextMonth DATETIME,
@SubGroupID INT,
@BaseTypeID INT
)
RETURNS DECIMAL(18, 2)
AS
BEGIN
	DECLARE @Res DECIMAL(18, 2) = ISNULL((
		SELECT
			SUM(ISNULL(i.transactionAmount, 0))
		FROM
			@order_items i
		WHERE
			i.theDate BETWEEN @TheMonth AND @NextMonth
			AND
			(@SubGroupID IS NULL OR i.EzBobCategory = @SubGroupID)
			AND
			i.transactionBaseTypeId = @BaseTypeID
	), 0)

	RETURN @Res
END
GO
