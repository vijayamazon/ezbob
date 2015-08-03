SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveAlibabaContract') IS NOT NULL
	DROP PROCEDURE SaveAlibabaContract
GO

IF TYPE_ID('AlibabaContractList') IS NOT NULL
	DROP TYPE AlibabaContractList
GO

CREATE TYPE AlibabaContractList AS TABLE (
	[RequestId] NVARCHAR(100) NULL,
	[ResponseId] NVARCHAR(100) NULL,
	[LoanId] BIGINT NULL,
	[OrderNumber] NVARCHAR(100) NULL,
	[ShippingMark] NVARCHAR(100) NULL,
	[TotalOrderAmount] INT NULL,
	[DeviationQuantityAllowed] INT NULL,
	[OrderAddtlDetails] NVARCHAR(100) NULL,
	[ShippingTerms] NVARCHAR(100) NULL,
	[ShippingDate] DATETIME NULL,
	[LoadingPort] NVARCHAR(100) NULL,
	[DestinationPort] NVARCHAR(100) NULL,
	[TACoveredAmount] INT NULL,
	[OrderDeposit] INT NULL,
	[OrderBalance] INT NULL,
	[OrderCurrency] NVARCHAR(100) NULL,
	[CommercialInvoice] VARBINARY(MAX) NULL,
	[BillOfLading] VARBINARY(MAX) NULL,
	[PackingList] VARBINARY(MAX) NULL,
	[Other] VARBINARY(MAX) NULL
)
GO

CREATE PROCEDURE SaveAlibabaContract
@Tbl AlibabaContractList READONLY
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @ContractID BIGINT

	INSERT INTO AlibabaContract (
		[RequestId],
		[ResponseId],
		[LoanId],
		[OrderNumber],
		[ShippingMark],
		[TotalOrderAmount],
		[DeviationQuantityAllowed],
		[OrderAddtlDetails],
		[ShippingTerms],
		[ShippingDate],
		[LoadingPort],
		[DestinationPort],
		[TACoveredAmount],
		[OrderDeposit],
		[OrderBalance],
		[OrderCurrency],
		[CommercialInvoice],
		[BillOfLading],
		[PackingList],
		[Other]
	) SELECT
		[RequestId],
		[ResponseId],
		[LoanId],
		[OrderNumber],
		[ShippingMark],
		[TotalOrderAmount],
		[DeviationQuantityAllowed],
		[OrderAddtlDetails],
		[ShippingTerms],
		[ShippingDate],
		[LoadingPort],
		[DestinationPort],
		[TACoveredAmount],
		[OrderDeposit],
		[OrderBalance],
		[OrderCurrency],
		[CommercialInvoice],
		[BillOfLading],
		[PackingList],
		[Other]
	FROM @Tbl
	
	SET @ContractID = SCOPE_IDENTITY()

	SELECT @ContractID AS ContractID
	
END
GO


