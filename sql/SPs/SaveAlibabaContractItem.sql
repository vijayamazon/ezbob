SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveAlibabaContractItem') IS NOT NULL
	DROP PROCEDURE SaveAlibabaContractItem
GO

IF TYPE_ID('AlibabaContractItemList') IS NOT NULL
	DROP TYPE AlibabaContractItemList
GO

CREATE TYPE AlibabaContractItemList AS TABLE (
	[ContractId] INT NULL,
	[OrderProdNumber] BIGINT NULL,
	[ProductName] NVARCHAR(100) NULL,
	[ProductSpecs] NVARCHAR(100) NULL,
	[ProductQuantity] INT NULL,
	[ProductUnit] INT NULL,
	[ProductUnitPrice] INT NULL,
	[ProductTotalAmount] INT NULL
)
GO

CREATE PROCEDURE SaveAlibabaContractItem
@Tbl AlibabaContractItemList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO AlibabaContractItem (
		[ContractId],
		[OrderProdNumber],
		[ProductName],
		[ProductSpecs],
		[ProductQuantity],
		[ProductUnit],
		[ProductUnitPrice],
		[ProductTotalAmount]
	) SELECT
		[ContractId],
		[OrderProdNumber],
		[ProductName],
		[ProductSpecs],
		[ProductQuantity],
		[ProductUnit],
		[ProductUnitPrice],
		[ProductTotalAmount]
	FROM @Tbl
END
GO


