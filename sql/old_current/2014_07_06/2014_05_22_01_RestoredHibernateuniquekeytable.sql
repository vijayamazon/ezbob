IF OBJECT_ID('hibernate_unique_key') IS NULL
BEGIN
	CREATE TABLE [dbo].[hibernate_unique_key](
		[next_hi] [int] NULL,
		[InventoryItemIdSeed] [int] NULL
	) ON [PRIMARY]
	INSERT INTO dbo.hibernate_unique_key
		(
		next_hi
		, InventoryItemIdSeed
		)
	VALUES
		(
		5379
		, 1005333
		)
END
GO





























