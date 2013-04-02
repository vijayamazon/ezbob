ALTER TABLE dbo.MP_EbayTransaction ADD
	ItemID nvarchar(128) NULL,
	ItemPrivateNotes nvarchar(MAX) NULL,
    ItemSellerInventoryID nvarchar(128) NULL,
	ItemSKU nvarchar(128) NULL,
	eBayTransactionId nvarchar(128) NULL,
	ItemInfoId int NULL
GO