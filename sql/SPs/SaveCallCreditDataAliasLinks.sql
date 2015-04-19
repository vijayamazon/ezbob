SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditDataAliasLinks') IS NOT NULL
	DROP PROCEDURE SaveCallCreditDataAliasLinks
GO

IF TYPE_ID('CallCreditDataAliasLinksList') IS NOT NULL
	DROP TYPE CallCreditDataAliasLinksList
GO

CREATE TYPE CallCreditDataAliasLinksList AS TABLE (
	[CallCreditDataID] BIGINT NULL,
	[Declared] BIT NULL,
	[NameBefore] NVARCHAR(164) NULL,
	[Alias] NVARCHAR(164) NULL,
	[CreationDate] DATETIME NULL,
	[LastConfDate] DATETIME NULL,
	[SupplierName] NVARCHAR(60) NULL,
	[SupplierTypeCode] NVARCHAR(10) NULL
)
GO

CREATE PROCEDURE SaveCallCreditDataAliasLinks
@Tbl CallCreditDataAliasLinksList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @CallCreditDataAliasLinksId BIGINT
	DECLARE @c INT

	SELECT @c = COUNT(*) FROM @Tbl

	IF @c = 0
		RAISERROR('Invalid argument: no/too much data to insert into SaveCallCreditDataAliasLinks table.', 11, 1)

	INSERT INTO CallCreditDataAliasLinks (
		[CallCreditDataID],
		[Declared],
		[NameBefore],
		[Alias],
		[CreationDate],
		[LastConfDate],
		[SupplierName],
		[SupplierTypeCode]
	) SELECT
		[CallCreditDataID],
		[Declared],
		[NameBefore],
		[Alias],
		[CreationDate],
		[LastConfDate],
		[SupplierName],
		[SupplierTypeCode]
	FROM @Tbl

	SET @CallCreditDataAliasLinksId = SCOPE_IDENTITY()

	SELECT @CallCreditDataAliasLinksId AS CallCreditDataAliasLinksId
END
GO


