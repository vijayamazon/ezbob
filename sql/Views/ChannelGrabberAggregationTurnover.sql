SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('ChannelGrabberAggregationTurnover') IS NOT NULL
	DROP VIEW ChannelGrabberAggregationTurnover
GO

CREATE VIEW ChannelGrabberAggregationTurnover AS
	SELECT
		AggID = a.ChannelGrabberAggregationID,
		a.TheMonth,
		a.IsActive,
		a.CustomerMarketPlaceUpdatingHistoryID,
		a.Turnover,
		h.CustomerMarketPlaceId,
		h.UpdatingEnd
	FROM
		ChannelGrabberAggregation a
		INNER JOIN MP_CustomerMarketplaceUpdatingHistory h
			ON a.CustomerMarketPlaceUpdatingHistoryID = h.Id
GO

