-- Depends on type PayPalOrderItems. Type's file drops this stored procedure.

IF OBJECT_ID('UpdateMpTotalsPayPal') IS NULL
	EXECUTE('CREATE PROCEDURE UpdateMpTotalsPayPal AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UpdateMpTotalsPayPal
@HistoryID INT
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @MpID INT

	EXECUTE GetMarketplaceFromHistoryID 'Pay Pal', @HistoryID, @MpID OUTPUT

	IF @MpID IS NULL
		RETURN

	------------------------------------------------------------------------------
	--
	-- Select relevant transactions.
	--
	------------------------------------------------------------------------------

	DECLARE @GbpID INT = dbo.udfGetDefaultCurrencyID()

	------------------------------------------------------------------------------

	CREATE TABLE #trn (
		Id INT,
		IsChosen BIT
	)

	------------------------------------------------------------------------------

	DECLARE @order_items PayPalOrderItems

	-- Step 1. Find relevant transactions.

	INSERT INTO #trn (Id, IsChosen)
	SELECT
		o.Id,
		CASE WHEN o.CustomerMarketPlaceUpdatingHistoryRecordId = @HistoryID THEN 1 ELSE 0 END
	FROM
		MP_PayPalTransaction o
	WHERE
		o.CustomerMarketPlaceId = @MpID

	-----------------------------------------------------------------------------

	DECLARE @MinDate DATETIME
	DECLARE @MaxDate DATETIME

	-----------------------------------------------------------------------------

	SELECT
		@MinDate = dbo.udfMonthStart(MIN(i.Created)),
		@MaxDate = dbo.udfMonthStart(MAX(i.Created))
	FROM
		MP_PayPalTransactionItem2 i
		INNER JOIN #trn o
			ON i.TransactionId = o.Id
			AND o.IsChosen = 1

	-----------------------------------------------------------------------------

	SET @MaxDate = DATEADD(second, -1, DATEADD(month, 1, @MaxDate))

	-----------------------------------------------------------------------------

	INSERT INTO @order_items (Id, Created, NetAmount, Type, Status)
	SELECT
		i.Id,
		i.Created,
		i.NetAmount * dbo.udfGetCurrencyRateByID(i.Created, i.CurrencyId, @GbpID),
		i.Type,
		i.Status
	FROM
		MP_PayPalTransactionItem2 i
		INNER JOIN #trn o
			ON i.TransactionId = o.Id
			AND i.Created BETWEEN @MinDate AND @MaxDate

	------------------------------------------------------------------------------
	--
	-- Create temp table for storing results.
	--
	------------------------------------------------------------------------------

	-- Kinda create table
	SELECT
		TheMonth,
		NextMonth = TheMonth,
		CustomerMarketPlaceUpdatingHistoryID,
		Turnover,
		GrossIncome,
		NetNumOfRefundsAndReturns,
		NetSumOfRefundsAndReturns,
		NetTransfersAmount,
		NumOfTotalTransactions,
		NumTransfersIn,
		NumTransfersOut,
		OutstandingBalance,
		RatioNetSumOfRefundsAndReturnsToNetRevenuesDenominator,
		RatioNetSumOfRefundsAndReturnsToNetRevenuesNumerator,
		RevenuesForTransactions,
		TotalNetExpenses,
		TotalNetInPayments,
		TotalNetOutPayments,
		TotalNetRevenues,
		TransactionsNumber,
		TransferAndWireIn,
		TransferAndWireOut,
		AmountPerTransferInNumerator,
		AmountPerTransferInDenominator,
		AmountPerTransferOutNumerator,
		AmountPerTransferOutDenominator,
		GrossProfitMarginNumerator,
		GrossProfitMarginDenominator,
		RevenuePerTrasnactionNumerator,
		RevenuePerTrasnactionDenominator
	INTO
		#months
	FROM
		PayPalAggregation
	WHERE
		1 = 0

	------------------------------------------------------------------------------
	--
	-- Extract single months from the relevant transactions.
	--
	------------------------------------------------------------------------------

	INSERT INTO #months (
		TheMonth,
		NextMonth,
		CustomerMarketPlaceUpdatingHistoryID,
		Turnover,
		GrossIncome,
		NetNumOfRefundsAndReturns,
		NetSumOfRefundsAndReturns,
		NetTransfersAmount,
		NumOfTotalTransactions,
		NumTransfersIn,
		NumTransfersOut,
		OutstandingBalance,
		RatioNetSumOfRefundsAndReturnsToNetRevenuesDenominator,
		RatioNetSumOfRefundsAndReturnsToNetRevenuesNumerator,
		RevenuesForTransactions,
		TotalNetExpenses,
		TotalNetInPayments,
		TotalNetOutPayments,
		TotalNetRevenues,
		TransactionsNumber,
		TransferAndWireIn,
		TransferAndWireOut,
		AmountPerTransferInNumerator,
		AmountPerTransferInDenominator,
		AmountPerTransferOutNumerator,
		AmountPerTransferOutDenominator,
		GrossProfitMarginNumerator,
		GrossProfitMarginDenominator,
		RevenuePerTrasnactionNumerator,
		RevenuePerTrasnactionDenominator
	)
	SELECT DISTINCT
		dbo.udfMonthStart(Created),
		'Jul 1 1976', -- Magic number because column ain't no allows null. It is replaced with the real value in the next query.
		@HistoryID,
		0, -- Turnover,
		0, -- GrossIncome,
		0, -- NetNumOfRefundsAndReturns,
		0, -- NetSumOfRefundsAndReturns,
		0, -- NetTransfersAmount,
		0, -- NumOfTotalTransactions,
		0, -- NumTransfersIn,
		0, -- NumTransfersOut,
		0, -- OutstandingBalance,
		0, -- RatioNetSumOfRefundsAndReturnsToNetRevenuesDenominator,
		0, -- RatioNetSumOfRefundsAndReturnsToNetRevenuesNumerator,
		0, -- RevenuesForTransactions,
		0, -- TotalNetExpenses,
		0, -- TotalNetInPayments,
		0, -- TotalNetOutPayments,
		0, -- TotalNetRevenues,
		0, -- TransactionsNumber,
		0, -- TransferAndWireIn,
		0, -- TransferAndWireOut,
		0, -- AmountPerTransferInNumerator,
		0, -- AmountPerTransferInDenominator,
		0, -- AmountPerTransferOutNumerator,
		0, -- AmountPerTransferOutDenominator,
		0, -- GrossProfitMarginNumerator,
		0, -- GrossProfitMarginDenominator,
		0, -- RevenuePerTrasnactionNumerator,
		0  -- RevenuePerTrasnactionDenominator
	FROM
		@order_items

	------------------------------------------------------------------------------

	UPDATE #months SET
		NextMonth = DATEADD(second, -1, DATEADD(month, 1, TheMonth))

	------------------------------------------------------------------------------
	--
	-- Calculation step 1: from raw data.
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		TotalNetInPayments        = dbo.udfPayPalFormula(@order_items, TheMonth, NextMonth, NULL, 'TotalNetInPayments'),        -- formula 101
		TotalNetOutPayments       = dbo.udfPayPalFormula(@order_items, TheMonth, NextMonth, NULL, 'TotalNetOutPayments'),       -- formula 102
		TransactionsNumber        = dbo.udfPayPalFormula(@order_items, TheMonth, NextMonth, NULL, 'TransactionsNumber'),        -- formula 103
		TotalNetRevenues          = dbo.udfPayPalFormula(@order_items, TheMonth, NextMonth, NULL, 'TotalNetRevenues'),          -- formula 1
		TotalNetExpenses          = dbo.udfPayPalFormula(@order_items, TheMonth, NextMonth, NULL, 'TotalNetExpenses'),          -- formula 2
		NumOfTotalTransactions    = dbo.udfPayPalFormula(@order_items, TheMonth, NextMonth, NULL, 'NumOfTotalTransactions'),    -- formula 3
		RevenuesForTransactions   = dbo.udfPayPalFormula(@order_items, TheMonth, NextMonth, NULL, 'RevenuesForTransactions'),   -- formula 4
		NetNumOfRefundsAndReturns = dbo.udfPayPalFormula(@order_items, TheMonth, NextMonth, NULL, 'NetNumOfRefundsAndReturns'), -- formula 5
		TransferAndWireOut        = dbo.udfPayPalFormula(@order_items, TheMonth, NextMonth, NULL, 'TransferAndWireOut'),        -- formula 6
		TransferAndWireIn         = dbo.udfPayPalFormula(@order_items, TheMonth, NextMonth, NULL, 'TransferAndWireIn'),         -- formula 7
		NetSumOfRefundsAndReturns = dbo.udfPayPalFormula(@order_items, TheMonth, NextMonth, NULL, 'NetNumOfRefundsAndReturns'), -- formula 5a
		NumTransfersOut           = dbo.udfPayPalFormula(@order_items, TheMonth, NextMonth,    0, 'TransferAndWireOut'),        -- formula 6a
		NumTransfersIn            = dbo.udfPayPalFormula(@order_items, TheMonth, NextMonth,    0, 'TransferAndWireIn')          -- formula 7a

	------------------------------------------------------------------------------
	--
	-- Calculation step 2: based on the previous step.
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		Turnover                         = TotalNetInPayments,
		GrossIncome                      = TotalNetRevenues - TotalNetExpenses,    -- formula 1a := formula 1 - formula 2
		NetTransfersAmount               = TransferAndWireOut + TransferAndWireIn, -- formula 4a := formula 6 + formula 7

		RevenuePerTrasnactionNumerator   = RevenuesForTransactions,
		RevenuePerTrasnactionDenominator = NumOfTotalTransactions                  -- formula 4 / formula 3

	------------------------------------------------------------------------------
	--
	-- Calculation step 3: based on the previous step.
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		OutstandingBalance                                     = GrossIncome - NetTransfersAmount, -- formula 1a - formula 4a

		GrossProfitMarginNumerator                             = GrossIncome,
		GrossProfitMarginDenominator                           = TotalNetRevenues,                 -- formula 1a / formula 1

		RatioNetSumOfRefundsAndReturnsToNetRevenuesNumerator   = NetSumOfRefundsAndReturns,
		RatioNetSumOfRefundsAndReturnsToNetRevenuesDenominator = TotalNetRevenues,                 -- forumla 5a / formula 1

		AmountPerTransferOutNumerator                          = TransferAndWireOut,
		AmountPerTransferOutDenominator                        = NumTransfersOut,                  -- formula 6 / formula 6a

		AmountPerTransferInNumerator                           = TransferAndWireIn,
		AmountPerTransferInDenominator                         = NumTransfersIn                    -- formula 7 / formula 7a

	------------------------------------------------------------------------------

	------------------------------------------------------------------------------
	--
	-- At this point table #months contains new data.
	--
	------------------------------------------------------------------------------

	SET TRANSACTION ISOLATION LEVEL SERIALIZABLE

	------------------------------------------------------------------------------

	BEGIN TRANSACTION

	------------------------------------------------------------------------------

	UPDATE PayPalAggregation SET
		IsActive = 0
	FROM
		PayPalAggregation a
		INNER JOIN MP_CustomerMarketPlaceUpdatingHistory h
			ON a.CustomerMarketplaceUpdatingHistoryID = h.Id
			AND h.CustomerMarketplaceID = @MpID
		INNER JOIN #months m ON a.TheMonth = m.TheMonth
	WHERE
		a.IsActive = 1

	------------------------------------------------------------------------------

	INSERT INTO PayPalAggregation (
		TheMonth,
		IsActive,
		CustomerMarketPlaceUpdatingHistoryID,
		Turnover,
		GrossIncome,
		NetNumOfRefundsAndReturns,
		NetSumOfRefundsAndReturns,
		NetTransfersAmount,
		NumOfTotalTransactions,
		NumTransfersIn,
		NumTransfersOut,
		OutstandingBalance,
		RatioNetSumOfRefundsAndReturnsToNetRevenuesDenominator,
		RatioNetSumOfRefundsAndReturnsToNetRevenuesNumerator,
		RevenuesForTransactions,
		TotalNetExpenses,
		TotalNetInPayments,
		TotalNetOutPayments,
		TotalNetRevenues,
		TransactionsNumber,
		TransferAndWireIn,
		TransferAndWireOut,
		AmountPerTransferInNumerator,
		AmountPerTransferInDenominator,
		AmountPerTransferOutNumerator,
		AmountPerTransferOutDenominator,
		GrossProfitMarginNumerator,
		GrossProfitMarginDenominator,
		RevenuePerTrasnactionNumerator,
		RevenuePerTrasnactionDenominator
	)
	SELECT
		TheMonth,
		1, -- IsActive
		CustomerMarketPlaceUpdatingHistoryID,
		Turnover,
		GrossIncome,
		NetNumOfRefundsAndReturns,
		NetSumOfRefundsAndReturns,
		NetTransfersAmount,
		NumOfTotalTransactions,
		NumTransfersIn,
		NumTransfersOut,
		OutstandingBalance,
		RatioNetSumOfRefundsAndReturnsToNetRevenuesDenominator,
		RatioNetSumOfRefundsAndReturnsToNetRevenuesNumerator,
		RevenuesForTransactions,
		TotalNetExpenses,
		TotalNetInPayments,
		TotalNetOutPayments,
		TotalNetRevenues,
		TransactionsNumber,
		TransferAndWireIn,
		TransferAndWireOut,
		AmountPerTransferInNumerator,
		AmountPerTransferInDenominator,
		AmountPerTransferOutNumerator,
		AmountPerTransferOutDenominator,
		GrossProfitMarginNumerator,
		GrossProfitMarginDenominator,
		RevenuePerTrasnactionNumerator,
		RevenuePerTrasnactionDenominator
	FROM
		#months

	------------------------------------------------------------------------------

	COMMIT TRANSACTION

	------------------------------------------------------------------------------
	--
	-- Clean up.
	--
	------------------------------------------------------------------------------

	DROP TABLE #months
	DROP TABLE #trn
END
GO
