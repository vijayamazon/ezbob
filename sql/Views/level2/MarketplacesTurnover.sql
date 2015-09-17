SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('MarketplaceTurnover') IS NOT NULL
	DROP VIEW MarketplaceTurnover
GO

CREATE VIEW MarketplaceTurnover AS
	SELECT
		t.AggID,
		t.TheMonth,
		t.IsActive,
		t.CustomerMarketPlaceUpdatingHistoryID,
		t.Turnover,
		t.CustomerMarketPlaceId,
		t.UpdatingEnd,
		mp.CustomerId,
		mp.Disabled AS IsMarketplaceDisabled,
		mt.InternalID AS MarketplaceInternalID,
		mt.IsPaymentAccount
	FROM
		(
			SELECT AggID, TheMonth, IsActive, CustomerMarketPlaceUpdatingHistoryID, Turnover, CustomerMarketPlaceId, UpdatingEnd FROM AmazonAggregationTurnover
			UNION
			SELECT AggID, TheMonth, IsActive, CustomerMarketPlaceUpdatingHistoryID, Turnover, CustomerMarketPlaceId, UpdatingEnd FROM ChannelGrabberAggregationTurnover
			UNION
			SELECT AggID, TheMonth, IsActive, CustomerMarketPlaceUpdatingHistoryID, Turnover, CustomerMarketPlaceId, UpdatingEnd FROM EbayAggregationTurnover
			UNION
			SELECT AggID, TheMonth, IsActive, CustomerMarketPlaceUpdatingHistoryID, Turnover, CustomerMarketPlaceId, UpdatingEnd FROM EkmAggregationTurnover
			UNION
			SELECT AggID, TheMonth, IsActive, CustomerMarketPlaceUpdatingHistoryID, Turnover, CustomerMarketPlaceId, UpdatingEnd FROM FreeAgentAggregationTurnover
			UNION
			SELECT AggID, TheMonth, IsActive, CustomerMarketPlaceUpdatingHistoryID, Turnover, CustomerMarketPlaceId, UpdatingEnd FROM HmrcAggregationTurnover
			UNION
			SELECT AggID, TheMonth, IsActive, CustomerMarketPlaceUpdatingHistoryID, Turnover, CustomerMarketPlaceId, UpdatingEnd FROM PayPalAggregationTurnover
			UNION
			SELECT AggID, TheMonth, IsActive, CustomerMarketPlaceUpdatingHistoryID, Turnover, CustomerMarketPlaceId, UpdatingEnd FROM PayPointAggregationTurnover
			UNION
			SELECT AggID, TheMonth, IsActive, CustomerMarketPlaceUpdatingHistoryID, Turnover, CustomerMarketPlaceId, UpdatingEnd FROM SageAggregationTurnover
			UNION
			SELECT AggID, TheMonth, IsActive, CustomerMarketPlaceUpdatingHistoryID, Turnover, CustomerMarketPlaceId, UpdatingEnd FROM YodleeAggregationTurnover
		) t
		INNER JOIN MP_CustomerMarketPlace mp ON t.CustomerMarketPlaceId = mp.Id
		INNER JOIN MP_MarketplaceType mt ON mp.MarketplaceId = mt.Id
GO
