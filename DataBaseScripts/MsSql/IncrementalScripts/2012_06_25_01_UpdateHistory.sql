ALTER TABLE MP_AmazonOrder ADD [CustomerMarketPlaceUpdatingHistoryRecordId] [int] NULL;
GO

ALTER TABLE MP_AmazonFeedback ADD [CustomerMarketPlaceUpdatingHistoryRecordId] [int] NULL;
GO

ALTER TABLE MP_AnalyisisFunctionValues ADD [CustomerMarketPlaceUpdatingHistoryRecordId] [int] NULL;
GO

ALTER TABLE MP_EbayAmazonInventory ADD [CustomerMarketPlaceUpdatingHistoryRecordId] [int] NULL;
GO

ALTER TABLE MP_EbayFeedback ADD [CustomerMarketPlaceUpdatingHistoryRecordId] [int] NULL;
GO

ALTER TABLE MP_EbayOrder ADD  [CustomerMarketPlaceUpdatingHistoryRecordId] [int] NULL;
GO

ALTER TABLE MP_EbayUserAccountData ADD [CustomerMarketPlaceUpdatingHistoryRecordId] [int] NULL;
GO

ALTER TABLE MP_EbayUserData ADD [CustomerMarketPlaceUpdatingHistoryRecordId] [int] NULL;
GO

ALTER TABLE MP_PayPalTransaction ADD [CustomerMarketPlaceUpdatingHistoryRecordId] [int] NULL;
GO
