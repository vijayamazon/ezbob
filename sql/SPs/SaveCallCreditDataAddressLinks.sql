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

	DECLARE @CallCreditDataAddressLinksId BIGINT
	DECLARE @c INT

	SELECT @c = COUNT(*) FROM @Tbl

	IF @c = 0
		RAISERROR('Invalid argument: no/too much data to insert into SaveCallCreditDataAddressLinks table.', 11, 1)

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

	SET @CallCreditDataAddressLinksId = SCOPE_IDENTITY()

	SELECT @CallCreditDataAddressLinksId AS CallCreditDataAddressLinksId
END
GO


