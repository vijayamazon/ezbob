IF OBJECT_ID('GetCustomerAnnualFcfValueAdded') IS NULL
	EXECUTE('CREATE PROCEDURE GetCustomerAnnualFcfValueAdded AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetCustomerAnnualFcfValueAdded
@CustomerID INT,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @HMRC UNIQUEIDENTIFIER = 'AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA'

	------------------------------------------------------------------------------

	CREATE TABLE #mp(
		MpID INT,
		LastUpdated DATETIME,
		CurrentMonth DATETIME,
		YearAgo DATETIME DEFAULT NULL
	)

	INSERT INTO #mp (MpID, LastUpdated, CurrentMonth)
	SELECT
		MpID = h.CustomerMarketPlaceId,
		LastUpdated = MAX(h.UpdatingEnd),
		CurrentMonth = dbo.udfMonthEnd(
			dbo.udfGetLatestTotalsMonth(h.CustomerMarketPlaceId, @Now)
		) -- with "month tail" consideration	
	FROM
		MP_CustomerMarketPlaceUpdatingHistory h
		INNER JOIN MP_CustomerMarketPlace m
			ON h.CustomerMarketPlaceId = m.Id
		INNER JOIN MP_MarketplaceType t
			ON m.MarketPlaceId = t.Id
			AND
			t.InternalId = @HMRC
	WHERE
		m.CustomerId = @CustomerID
		AND
		h.UpdatingEnd <= @Now
		AND
		ISNULL(m.Disabled, 0) = 0
		AND
		(LEN(h.Error) = 0 OR h.Error IS NULL)
	GROUP BY
		h.CustomerMarketPlaceId

	IF NOT EXISTS(SELECT * FROM #mp)
	BEGIN
		DROP TABLE #mp
		RETURN
	END

	------------------------------------------------------------------------------

	UPDATE #mp SET
		YearAgo = dbo.udfMonthStart(DATEADD(month, -11, CurrentMonth))

	------------------------------------------------------------------------------

	SELECT
		t.FreeCashFlow,
		t.ValueAdded,
		t.TheMonth,
		m.CurrentMonth,
		m.MpID,
		RowNum = ROW_NUMBER() OVER (
			PARTITION BY m.MpID, t.TheMonth
			ORDER BY t.CustomerMarketPlaceUpdatingHistoryID DESC
		)
	INTO
		#raw
	FROM
		HmrcAggregation t
		INNER JOIN MP_CustomerMarketPlaceUpdatingHistory h
			ON t.CustomerMarketPlaceUpdatingHistoryID = h.Id
		INNER JOIN #mp m
			ON h.CustomerMarketPlaceId = m.MpID
			AND t.TheMonth BETWEEN m.YearAgo AND m.CurrentMonth

	------------------------------------------------------------------------------

	DECLARE @Factor DECIMAL(18, 2) = 0
	DECLARE @NewActualLoansRepayment DECIMAL(18, 2) = 0
	DECLARE @CurrentBalance DECIMAL(18, 2) = 0

	BEGIN TRY
		SELECT
			@Factor = CONVERT(DECIMAL(18, 2), Value)
		FROM
			ConfigurationVariables
		WHERE
			Name = 'FCFFactor'
	END TRY
	BEGIN CATCH
		SET @Factor = 0
	END CATCH

	IF @Factor IS NOT NULL AND @Factor != 0
	BEGIN
		SELECT
			@CurrentBalance = cac.CurrentBalanceSum
		FROM
			dbo.udfGetCustomerCompanyAnalytics(@CustomerId, @Now, 1, 0, 0) cac

		IF @CurrentBalance IS NULL
			SET @CurrentBalance = 0

		SET @NewActualLoansRepayment = @CurrentBalance / @Factor

		IF @NewActualLoansRepayment < 0
			SET @NewActualLoansRepayment = 0
	END

	------------------------------------------------------------------------------

	SELECT
		RowType = 'NewActualLoansRepayment',
		NewActualLoansRepayment = @NewActualLoansRepayment

	SELECT
		RowType = 'FcfValueAdded',
		r.FreeCashFlow,
		r.ValueAdded,
		r.TheMonth,
		Distance = DATEDIFF(month, r.TheMonth, r.CurrentMonth),
		r.CurrentMonth,
		r.MpID
	FROM
		#raw r
	WHERE
		r.RowNum = 1

	------------------------------------------------------------------------------

	DROP TABLE #raw
	DROP TABLE #mp
END
GO
