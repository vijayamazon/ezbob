SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('GetYodleeRevenues') IS NULL
	EXECUTE('CREATE PROCEDURE GetYodleeRevenues AS SELECT 1')
GO

ALTER PROCEDURE GetYodleeRevenues
@CustomerMarketplaceId INT,
@DateFrom DATETIME,
@DateTo DATETIME,
@YodleeRevenues DECIMAL(18,4) OUTPUT,
@IsParsedBank BIT OUTPUT,
@MinDate DATETIME OUTPUT,
@MaxDate DATETIME OUTPUT,
@TranDayCount INT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @LongAgo DATETIME = 'April 5 1753'

	IF @DateFrom IS NULL OR @DateTo IS NULL
	BEGIN
		SET @DateFrom = NULL
		SET @DateTo = NULL
	END

	SET @YodleeRevenues = 0
	SET @IsParsedBank = 0
	SET @TranDayCount = 0

	DECLARE @MinPostDate DATETIME
	DECLARE @MinTransDate DATETIME

	SELECT
		@IsParsedBank = CASE mp.DisplayName WHEN 'ParsedBank' THEN 1 ELSE 0 END
	FROM
		MP_CustomerMarketPlace mp
	WHERE
		mp.Id = @CustomerMarketplaceId

	IF @IsParsedBank = 0 
	BEGIN
		;WITH trns AS (
			SELECT DISTINCT
				tr.srcElementId srcElementId,
				tr.Id
			FROM
				MP_CustomerMarketPlace mp 
				INNER JOIN MP_YodleeOrder o ON o.CustomerMarketPlaceId = mp.Id
				INNER JOIN MP_YodleeOrderItem i ON i.OrderId = o.Id
				INNER JOIN MP_YodleeOrderItemBankTransaction tr ON tr.OrderItemId = i.Id
				INNER JOIN MP_YodleeGroup g ON g.Id = tr.EzbobCategory
			WHERE
				mp.Id = @CustomerMarketplaceId
				AND
				tr.transactionBaseType = 'credit'
				AND
				ISNULL(tr.isSeidMod, 0) = 0
				AND
				g.MainGroup = 'Revenues'
				AND (
					@DateFrom IS NULL
					OR
					(tr.transactionDate BETWEEN @DateFrom AND @DateTo)
					OR
					(tr.postDate BETWEEN @DateFrom AND @DateTo)
				)
		)
		SELECT
			@YodleeRevenues = SUM(ISNULL(tr.transactionAmount, 0)),
			@TranDayCount = COUNT(DISTINCT CONVERT(DATE, ISNULL(tr.transactionDate, tr.postDate)))
		FROM
			MP_YodleeOrderItemBankTransaction tr
			INNER JOIN trns ON tr.Id = trns.Id

		SELECT
			@MaxDate = MAX(i.asOfDate),
			@MinTransDate = MIN(ISNULL(ISNULL(tr.transactionDate, tr.postDate), @LongAgo)),
			@MinPostDate = MIN(ISNULL(ISNULL(tr.postDate, tr.transactionDate), @LongAgo))
		FROM
			MP_CustomerMarketPlace mp 
			INNER JOIN MP_YodleeOrder o ON o.CustomerMarketPlaceId = mp.Id
			INNER JOIN MP_YodleeOrderItem i ON i.OrderId = o.Id
			LEFT JOIN MP_YodleeOrderItemBankTransaction tr ON tr.OrderItemId = i.Id
		WHERE
			mp.Id = @CustomerMarketplaceId
			AND (
				@DateFrom IS NULL
				OR
				(tr.transactionDate BETWEEN @DateFrom AND @DateTo)
				OR
				(tr.postDate BETWEEN @DateFrom AND @DateTo)
			)

		IF @MinTransDate = @LongAgo
		BEGIN
			SET @MinTransDate = NULL
			SET @MinPostDate = NULL
		END

		SET @MinDate = dbo.udfMinDate(@MinPostDate, @MinTransDate)
	END
	ELSE	BEGIN
		;WITH lastOrder AS (
			SELECT
				MAX(o.Id) Id
			FROM
				MP_CustomerMarketPlace mp 
				INNER JOIN MP_YodleeOrder o ON o.CustomerMarketPlaceId = mp.Id
			WHERE
				mp.Id = @CustomerMarketplaceId
				AND
				mp.Disabled = 0
		)
		SELECT
			@YodleeRevenues = SUM(ISNULL(tr.transactionAmount, 0)),
			@TranDayCount = COUNT(DISTINCT CONVERT(DATE, tr.transactionDate))
		FROM
			MP_YodleeOrderItemBankTransaction tr 
			INNER JOIN MP_YodleeOrderItem i ON i.Id = tr.OrderItemId 
			INNER JOIN MP_YodleeOrder o ON o.Id = i.OrderId
			INNER JOIN lastOrder ON o.Id = lastOrder.Id
			INNER JOIN MP_YodleeGroup g ON g.Id = tr.EzbobCategory
		WHERE
			tr.transactionBaseType = 'credit'
			AND
			(tr.isSeidMod = 0 OR tr.isSeidMod IS NULL)
			AND
			g.MainGroup = 'Revenues'
			AND (
				@DateFrom IS NULL
				OR
				(tr.transactionDate BETWEEN @DateFrom AND @DateTo)
			)

		;WITH lastOrder AS (
			SELECT
				MAX(o.Id) Id
			FROM
				MP_CustomerMarketPlace mp 
				INNER JOIN MP_YodleeOrder o ON o.CustomerMarketPlaceId = mp.Id
			WHERE
				mp.Id = @CustomerMarketplaceId
				AND
				mp.Disabled = 0
		)
		SELECT
			@MinDate = MIN(ISNULL(tr.transactionDate, @LongAgo)),
			@MaxDate = MAX(i.asOfDate)
		FROM
			MP_YodleeOrderItemBankTransaction tr 
			INNER JOIN MP_YodleeOrderItem i ON i.Id = tr.OrderItemId 
			INNER JOIN MP_YodleeOrder o ON o.Id = i.OrderId
			INNER JOIN lastOrder ON o.Id = lastOrder.Id
		WHERE
			@DateFrom IS NULL
			OR
			(tr.transactionDate BETWEEN @DateFrom AND @DateTo)

		IF @MinDate = @LongAgo
			SET @MinDate = NULL
	END

	SET @YodleeRevenues = ISNULL(@YodleeRevenues, 0)
	SET @TranDayCount = ISNULL(@TranDayCount, 0)
END
GO
