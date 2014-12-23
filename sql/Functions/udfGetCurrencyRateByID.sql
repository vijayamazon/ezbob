IF OBJECT_ID('dbo.udfGetCurrencyRateByID') IS NOT NULL
	DROP FUNCTION dbo.udfGetCurrencyRateByID
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION dbo.udfGetCurrencyRateByID(
@TheDate DATETIME,
@CurrencyID INT,
@DefaultCurrencyID INT
)
RETURNS DECIMAL(18, 8)
AS
BEGIN
	IF @CurrencyID IS NULL
		RETURN CONVERT(DECIMAL(18, 8), 1)

	IF @DefaultCurrencyID IS NOT NULL AND @CurrencyID = @DefaultCurrencyID
		RETURN CONVERT(DECIMAL(18, 8), 1)

	DECLARE @Res DECIMAL(18, 8)

	IF @TheDate IS NULL
	BEGIN
		SELECT
			@Res = c.Price
		FROM
			MP_Currency c
		WHERE
			c.Id = @CurrencyID
	END
	ELSE BEGIN
		SELECT TOP 1
			@Res = h.Price
		FROM
			MP_CurrencyRateHistory h
		WHERE
			h.CurrencyId = @CurrencyID
			AND
			h.Updated <= @TheDate
		ORDER BY
			h.Updated DESC
	END

	RETURN ISNULL(@Res, 1)
END
GO
