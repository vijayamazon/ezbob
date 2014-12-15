SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('HmrcAggregationTurnover') IS NOT NULL
	DROP VIEW HmrcAggregationTurnover
GO

CREATE VIEW HmrcAggregationTurnover AS
	SELECT
		a.TheMonth,
		a.IsActive,
		a.CustomerMarketPlaceUpdatingHistoryID,
		a.Turnover,
		h.CustomerMarketPlaceId,
		h.UpdatingEnd
	FROM
		HmrcAggregation a
		INNER JOIN MP_CustomerMarketplaceUpdatingHistory h
			ON a.CustomerMarketPlaceUpdatingHistoryID = h.Id
GO
