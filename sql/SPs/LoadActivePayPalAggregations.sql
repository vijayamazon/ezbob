SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadActivePayPalAggregations') IS NULL
	EXECUTE('CREATE PROCEDURE LoadActivePayPalAggregations AS SELECT 1')
GO

ALTER PROCEDURE LoadActivePayPalAggregations
@MpID INT,
@RelevantDate DATETIME
AS
BEGIN
	SELECT
		a.PayPalAggregationID,
		a.TheMonth,
		a.IsActive,
		a.Turnover,
		a.GrossIncome,
		a.NetNumOfRefundsAndReturns,
		a.NetSumOfRefundsAndReturns,
		a.NetTransfersAmount,
		a.NumOfTotalTransactions,
		a.NumTransfersIn,
		a.NumTransfersOut,
		a.OutstandingBalance,
		a.RatioNetSumOfRefundsAndReturnsToNetRevenuesDenominator,
		a.RatioNetSumOfRefundsAndReturnsToNetRevenuesNumerator,
		a.RevenuesForTransactions,
		a.TotalNetExpenses,
		a.TotalNetInPayments,
		a.TotalNetOutPayments,
		a.TotalNetRevenues,
		a.TransactionsNumber,
		a.TransferAndWireIn,
		a.TransferAndWireOut,
		a.AmountPerTransferInNumerator,
		a.AmountPerTransferInDenominator,
		a.AmountPerTransferOutNumerator,
		a.AmountPerTransferOutDenominator,
		a.GrossProfitMarginNumerator,
		a.GrossProfitMarginDenominator,
		a.RevenuePerTrasnactionNumerator,
		a.RevenuePerTrasnactionDenominator
	FROM
		PayPalAggregation a
		INNER JOIN MP_CustomerMarketPlaceUpdatingHistory h
			ON h.Id = a.CustomerMarketPlaceUpdatingHistoryID
			AND h.CustomerMarketPlaceId = @MpID
	WHERE
		a.IsActive = 1
		AND
		a.TheMonth < @RelevantDate
	ORDER BY
		a.TheMonth DESC
END
GO
