IF OBJECT_ID('GetCustomerAnnualTurnover') IS NULL
	EXECUTE('CREATE PROCEDURE GetCustomerAnnualTurnover AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetCustomerAnnualTurnover
@CustomerID INT,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	SELECT
		m.Id,
		CurrentMonth = dbo.udfMonthEnd(dbo.udfGetLatestTotalsMonth(m.Id, @Now)),
		YearAgo = CONVERT(DATETIME, NULL)
	INTO
		#mp
	FROM
		MP_CustomerMarketPlace m
	WHERE
		m.CustomerId = @CustomerID
		AND
		m.UpdatingEnd IS NOT NULL
		AND
		ISNULL(m.Disabled, 0) = 0
		AND
		LTRIM(RTRIM(ISNULL(m.UpdateError, ''))) = ''

	------------------------------------------------------------------------------

	UPDATE #mp SET
		YearAgo = dbo.udfMonthStart(DATEADD(month, -11, CurrentMonth))

	------------------------------------------------------------------------------

	DECLARE @Turnover NUMERIC(18, 2)

	------------------------------------------------------------------------------

	SELECT
		@Turnover = SUM(t.Turnover)
	FROM
		MarketplaceTurnover t
		INNER JOIN #mp
			ON t.CustomerMarketPlaceId = #mp.Id
			AND t.TheMonth BETWEEN #mp.YearAgo AND #mp.CurrentMonth
	WHERE
		t.IsActive = 1

	------------------------------------------------------------------------------

	SELECT Turnover = ISNULL(@Turnover, 0)

	------------------------------------------------------------------------------

	DROP TABLE #mp
END
GO
