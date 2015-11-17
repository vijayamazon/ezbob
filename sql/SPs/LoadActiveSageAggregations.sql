SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadActiveSageAggregations') IS NULL
	EXECUTE('CREATE PROCEDURE LoadActiveSageAggregations AS SELECT 1')
GO

ALTER PROCEDURE LoadActiveSageAggregations
@MpID INT,
@RelevantDate DATETIME
AS
BEGIN
	SELECT
		a.SageAggregationID,
		a.TheMonth,
		a.IsActive,
		a.Turnover,
		a.NumOfExpenditures,
		a.NumOfIncomes,
		a.NumOfOrders,
		a.NumOfPurchaseInvoices,
		a.TotalSumOfExpenditures,
		a.TotalSumOfIncomes,
		a.TotalSumOfOrders,
		a.TotalSumOfPaidPurchaseInvoices,
		a.TotalSumOfPaidSalesInvoices,
		a.TotalSumOfPartiallyPaidPurchaseInvoices,
		a.TotalSumOfPartiallyPaidSalesInvoices,
		a.TotalSumOfPurchaseInvoices,
		a.TotalSumOfUnpaidPurchaseInvoices,
		a.TotalSumOfUnpaidSalesInvoices
	FROM
		SageAggregation a
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
