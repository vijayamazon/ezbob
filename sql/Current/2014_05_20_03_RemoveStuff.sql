IF OBJECT_ID('MP_EbayAmazonInventoryItem') IS NOT NULL
BEGIN
	DROP TABLE MP_EbayAmazonInventoryItem
END
GO
IF OBJECT_ID('MP_EbayAmazonInventory') IS NOT NULL
BEGIN
	DROP TABLE MP_EbayAmazonInventory
END
GO

IF OBJECT_ID (N'dbo.hibernate_unique_key') IS NOT NULL
	DROP TABLE dbo.hibernate_unique_key
GO

IF OBJECT_ID('MP_AmazonOrderItem') IS NOT NULL
	DROP TABLE MP_AmazonOrderItem
GO


