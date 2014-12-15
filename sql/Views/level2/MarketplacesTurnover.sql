SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('MarketplaceTurnover') IS NOT NULL
	DROP VIEW MarketplaceTurnover
GO

CREATE VIEW MarketplaceTurnover AS
	SELECT TheMonth, IsActive, CustomerMarketPlaceUpdatingHistoryID, Turnover, CustomerMarketPlaceId, UpdatingEnd FROM AmazonAggregationTurnover
	UNION
	SELECT TheMonth, IsActive, CustomerMarketPlaceUpdatingHistoryID, Turnover, CustomerMarketPlaceId, UpdatingEnd FROM ChannelGrabberAggregationTurnover
	UNION
	SELECT TheMonth, IsActive, CustomerMarketPlaceUpdatingHistoryID, Turnover, CustomerMarketPlaceId, UpdatingEnd FROM EbayAggregationTurnover
	UNION
	SELECT TheMonth, IsActive, CustomerMarketPlaceUpdatingHistoryID, Turnover, CustomerMarketPlaceId, UpdatingEnd FROM EkmAggregationTurnover
	UNION
	SELECT TheMonth, IsActive, CustomerMarketPlaceUpdatingHistoryID, Turnover, CustomerMarketPlaceId, UpdatingEnd FROM FreeAgentAggregationTurnover
	UNION
	SELECT TheMonth, IsActive, CustomerMarketPlaceUpdatingHistoryID, Turnover, CustomerMarketPlaceId, UpdatingEnd FROM HmrcAggregationTurnover
	UNION
	SELECT TheMonth, IsActive, CustomerMarketPlaceUpdatingHistoryID, Turnover, CustomerMarketPlaceId, UpdatingEnd FROM PayPalAggregationTurnover
	UNION
	SELECT TheMonth, IsActive, CustomerMarketPlaceUpdatingHistoryID, Turnover, CustomerMarketPlaceId, UpdatingEnd FROM PayPointAggregationTurnover
	UNION
	SELECT TheMonth, IsActive, CustomerMarketPlaceUpdatingHistoryID, Turnover, CustomerMarketPlaceId, UpdatingEnd FROM SageAggregationTurnover
	UNION
	SELECT TheMonth, IsActive, CustomerMarketPlaceUpdatingHistoryID, Turnover, CustomerMarketPlaceId, UpdatingEnd FROM YodleeAggregationTurnover
GO
