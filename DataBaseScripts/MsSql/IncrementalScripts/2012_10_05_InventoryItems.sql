GO
ALTER TABLE dbo.hibernate_unique_key ADD
	InventoryItemIdSeed int NULL
GO

UPDATE [dbo].[hibernate_unique_key]
   SET [InventoryItemIdSeed] = 1000000
GO
CREATE TABLE dbo.Tmp_MP_EbayAmazonInventoryItem
	(
	Id int NOT NULL,
	InventoryId int NOT NULL,
	BidCount int NULL,
	Sku nvarchar(128) NULL,
	Price float(53) NULL,
	Quantity int NULL,
	ItemId nvarchar(128) NULL,
	Currency nvarchar(10) NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_MP_EbayAmazonInventoryItem SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM dbo.MP_EbayAmazonInventoryItem)
	 EXEC('INSERT INTO dbo.Tmp_MP_EbayAmazonInventoryItem (Id, InventoryId, BidCount, Sku, Price, Quantity, ItemId, Currency)
		SELECT Id, InventoryId, BidCount, Sku, Price, Quantity, ItemId, Currency FROM dbo.MP_EbayAmazonInventoryItem WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE dbo.MP_EbayAmazonInventoryItem
GO
EXECUTE sp_rename N'dbo.Tmp_MP_EbayAmazonInventoryItem', N'MP_EbayAmazonInventoryItem', 'OBJECT' 
GO
ALTER TABLE dbo.MP_EbayAmazonInventoryItem ADD CONSTRAINT
	PK_MP_InventoryItem PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX IX_MP_EbayAmazonInventoryItemCurrency ON dbo.MP_EbayAmazonInventoryItem
	(
	Currency
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_MP_EbayAmazonInventoryItemInventoryId ON dbo.MP_EbayAmazonInventoryItem
	(
	InventoryId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_MP_EbayAmazonInventoryItemPrice ON dbo.MP_EbayAmazonInventoryItem
	(
	Price
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_MP_EbayAmazonInventoryItemQuantity ON dbo.MP_EbayAmazonInventoryItem
	(
	Quantity
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE dbo.MP_EbayAmazonInventoryItem ADD CONSTRAINT
	FK_MP_InventoryItem_MP_Inventory FOREIGN KEY
	(
	InventoryId
	) REFERENCES dbo.MP_EbayAmazonInventory
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO