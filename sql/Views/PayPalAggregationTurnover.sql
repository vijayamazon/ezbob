SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('PayPalAggregationTurnover') IS NOT NULL
	DROP VIEW PayPalAggregationTurnover
GO

CREATE VIEW PayPalAggregationTurnover AS
	SELECT
		a.TheMonth,
		a.IsActive,
		a.CustomerMarketPlaceUpdatingHistoryID,
		a.Turnover,
		h.CustomerMarketPlaceId,
		h.UpdatingEnd
	FROM
		PayPalAggregation a
		INNER JOIN MP_CustomerMarketplaceUpdatingHistory h
			ON a.CustomerMarketPlaceUpdatingHistoryID = h.Id
GO

