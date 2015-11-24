SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadActiveFreeAgentAggregations') IS NULL
	EXECUTE('CREATE PROCEDURE LoadActiveFreeAgentAggregations AS SELECT 1')
GO

ALTER PROCEDURE LoadActiveFreeAgentAggregations
@MpID INT,
@RelevantDate DATETIME
AS
BEGIN
	SELECT
		a.FreeAgentAggregationID,
		a.TheMonth,
		a.IsActive,
		a.Turnover,
		a.NumOfExpenses,
		a.NumOfOrders,
		a.SumOfAdminExpensesCategory,
		a.SumOfCostOfSalesExpensesCategory,
		a.SumOfDraftInvoices,
		a.SumOfGeneralExpensesCategory,
		a.SumOfOpenInvoices,
		a.SumOfOverdueInvoices,
		a.SumOfPaidInvoices,
		a.TotalSumOfExpenses,
		a.TotalSumOfOrders
	FROM
		FreeAgentAggregation a
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
