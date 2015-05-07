SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('FreeAgentAggregationTurnover') IS NOT NULL
	DROP VIEW FreeAgentAggregationTurnover
GO

CREATE VIEW FreeAgentAggregationTurnover AS
	SELECT
		AggID = a.FreeAgentAggregationID,
		a.TheMonth,
		a.IsActive,
		a.CustomerMarketPlaceUpdatingHistoryID,
		a.Turnover,
		h.CustomerMarketPlaceId,
		h.UpdatingEnd
	FROM
		FreeAgentAggregation a
		INNER JOIN MP_CustomerMarketplaceUpdatingHistory h
			ON a.CustomerMarketPlaceUpdatingHistoryID = h.Id
GO

