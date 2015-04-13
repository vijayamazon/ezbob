SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditDataAddressLinks') IS NOT NULL
	DROP PROCEDURE SaveCallCreditDataAddressLinks
GO

IF TYPE_ID('CallCreditDataAddressLinksList') IS NOT NULL
	DROP TYPE CallCreditDataAddressLinksList
GO

CREATE TYPE CallCreditDataAddressLinksList AS TABLE (
	[CallCreditDataID] BIGINT NULL,
	[CreationDate] DATETIME NULL,
	[LastConfDate] DATETIME NULL,
	[From] INT NULL,
	[To] INT NULL,
	[SupplierName] NVARCHAR(60) NULL,
	[SupplierTypeCode] NVARCHAR(10) NULL
)
GO

CREATE PROCEDURE SaveCallCreditDataAddressLinks
@Tbl CallCreditDataAddressLinksList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CallCreditDataAddressLinks (
		[CallCreditDataID],
		[CreationDate],
		[LastConfDate],
		[From],
		[To],
		[SupplierName],
		[SupplierTypeCode]
	) SELECT
		[CallCreditDataID],
		[CreationDate],
		[LastConfDate],
		[From],
		[To],
		[SupplierName],
		[SupplierTypeCode]
	FROM @Tbl
END
GO


