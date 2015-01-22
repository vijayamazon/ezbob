IF OBJECT_ID('GetCustomerTurnoverForAutoDecision') IS NULL
	EXECUTE('CREATE PROCEDURE GetCustomerTurnoverForAutoDecision AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetCustomerTurnoverForAutoDecision
@IsForApprove BIT,
@CustomerID INT,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @eBay   UNIQUEIDENTIFIER = 'A7120CB7-4C93-459B-9901-0E95E7281B59'
	DECLARE @Amazon UNIQUEIDENTIFIER = 'A4920125-411F-4BB9-A52D-27E8A00D0A3B'
	DECLARE @PayPal UNIQUEIDENTIFIER = '3FA5E327-FCFD-483B-BA5A-DC1815747A28'
	DECLARE @Yodlee UNIQUEIDENTIFIER = '107DE9EB-3E57-4C5B-A0B5-FFF445C4F2DF'
	DECLARE @HMRC   UNIQUEIDENTIFIER = 'AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA'

	------------------------------------------------------------------------------

	CREATE TABLE #mp(
		MpID int,
		LastUpdated DATETIME,
		CurrentMonth DATETIME,
		YearAgo DATETIME default NULL,
		MpTypeID UNIQUEIDENTIFIER default NULL,
		IsPaymentAccount bit default NULL 
	)

	INSERT INTO #mp (MpID,LastUpdated,CurrentMonth )
	SELECT
		MpID = h.CustomerMarketPlaceId,
		LastUpdated = MAX(h.UpdatingEnd),
		CurrentMonth = dbo.udfMonthEnd(
			dbo.udfGetLatestTotalsMonth(h.CustomerMarketPlaceId, @Now)
		)	-- with "month tail" consideration	
	FROM
		dbo.MP_CustomerMarketPlaceUpdatingHistory h
		INNER JOIN dbo.MP_CustomerMarketPlace m
			ON h.CustomerMarketPlaceId = m.Id
			AND m.CustomerId = @CustomerID
		INNER JOIN dbo.MP_MarketplaceType t
			ON m.MarketPlaceId = t.Id
			AND (
				ISNULL(@IsForApprove, 0) = 0
				OR
				t.InternalId IN (@HMRC, @Yodlee, @eBay, @Amazon, @PayPal)
			)
	WHERE
		h.UpdatingEnd <= @Now
		AND
		ISNULL(m.Disabled, 0) = 0
		AND (
			h.Error IS NULL
			OR
			LTRIM(RTRIM(h.Error)) = ''
		)
	GROUP BY
		h.CustomerMarketPlaceId

	-- RELEVANT RECORDS NOT FOUND
	IF (SELECT COUNT(*) FROM #mp ) = 0
	BEGIN
		DROP TABLE #mp
		RETURN
	END

	------------------------------------------------------------------------------

	UPDATE #mp SET
		MpTypeID = mt.InternalId,		
		IsPaymentAccount = mt.IsPaymentAccount,
		YearAgo = dbo.udfMonthStart(DATEADD(month, -11, CurrentMonth))
	FROM
		#mp
		INNER JOIN dbo.MP_CustomerMarketPlace m ON #mp.MpID = m.Id
		INNER JOIN dbo.MP_MarketplaceType mt ON m.MarketPlaceId = mt.Id

	------------------------------------------------------------------------------

	SELECT
		t.Turnover,
		t.TheMonth,
		m.CurrentMonth,
		m.MpID,
		m.MpTypeID,
		m.IsPaymentAccount,
		RowNum = ROW_NUMBER() OVER (
			PARTITION BY m.MpID, t.TheMonth
			ORDER BY t.CustomerMarketPlaceUpdatingHistoryID DESC
		)
	INTO
		#raw
	FROM
		dbo.MarketplaceTurnover t
		INNER JOIN #mp m
			ON t.CustomerMarketPlaceId = m.MpID
			AND t.TheMonth BETWEEN m.YearAgo AND m.CurrentMonth

	-- RELEVANT RECORDS NOT FOUND
	IF (SELECT COUNT(*) FROM #raw ) = 0 
	BEGIN
		DROP TABLE #raw
		DROP TABLE #mp
		RETURN
	END

	------------------------------------------------------------------------------

	SELECT
		RowType = 'Turnover',
		r.Turnover,
		r.TheMonth,
		Distance = DATEDIFF(month, r.TheMonth, r.CurrentMonth),
		r.CurrentMonth,
		r.MpID,
		r.MpTypeID,
		r.IsPaymentAccount
	FROM
		#raw r
	WHERE
		r.RowNum = 1

	------------------------------------------------------------------------------

	DROP TABLE #raw
	DROP TABLE #mp
END
GO
