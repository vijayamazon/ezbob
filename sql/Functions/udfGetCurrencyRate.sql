IF OBJECT_ID('dbo.udfGetCurrencyRate') IS NOT NULL
	DROP FUNCTION dbo.udfGetCurrencyRate
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION dbo.udfGetCurrencyRate(
@TheDate DATETIME,
@TheCurrency NVARCHAR(10)
)
RETURNS DECIMAL(18, 8)
AS
BEGIN
	IF @TheCurrency IS NULL OR @TheCurrency = 'GBP'
		RETURN CONVERT(DECIMAL(18, 8), 1)

	IF @TheCurrency = 'GBX'
		RETURN CONVERT(DECIMAL(18, 8), 0.01)

	DECLARE @Res DECIMAL(18, 8)

	IF @TheDate IS NULL
	BEGIN
		SELECT
			@Res = c.Price
		FROM
			MP_Currency c
		WHERE
			c.Name = @TheCurrency
	END
	ELSE BEGIN
		SELECT TOP 1
			@Res = h.Price
		FROM
			MP_CurrencyRateHistory h
			INNER JOIN MP_Currency c
				ON h.CurrencyId = c.Id
				AND c.Name = @TheCurrency
		WHERE
			h.Updated <= @TheDate
		ORDER BY
			h.Updated DESC
	END

	SET @Res = ISNULL(@Res, 1)

	SET @Res = CASE WHEN @Res = 0 THEN 1 ELSE 1 / @Res END

	RETURN @Res
END
GO
