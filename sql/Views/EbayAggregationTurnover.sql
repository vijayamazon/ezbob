SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('EbayAggregationTurnover') IS NOT NULL
	DROP VIEW EbayAggregationTurnover
GO

CREATE VIEW EbayAggregationTurnover AS
	SELECT
		AggID = a.EbayAggregationID,
		a.TheMonth,
		a.IsActive,
		a.CustomerMarketPlaceUpdatingHistoryID,
		a.Turnover,
		h.CustomerMarketPlaceId,
		h.UpdatingEnd
	FROM
		EbayAggregation a
		INNER JOIN MP_CustomerMarketplaceUpdatingHistory h
			ON a.CustomerMarketPlaceUpdatingHistoryID = h.Id
GO

