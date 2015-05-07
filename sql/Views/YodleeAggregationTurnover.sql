SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('YodleeAggregationTurnover') IS NOT NULL
	DROP VIEW YodleeAggregationTurnover
GO

CREATE VIEW YodleeAggregationTurnover AS
	SELECT
		AggID = a.YodleeAggregationID,
		a.TheMonth,
		a.IsActive,
		a.CustomerMarketPlaceUpdatingHistoryID,
		a.Turnover,
		h.CustomerMarketPlaceId,
		h.UpdatingEnd
	FROM
		YodleeAggregation a
		INNER JOIN MP_CustomerMarketplaceUpdatingHistory h
			ON a.CustomerMarketPlaceUpdatingHistoryID = h.Id
GO
