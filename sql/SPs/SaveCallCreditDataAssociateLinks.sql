SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditDataAssociateLinks') IS NOT NULL
	DROP PROCEDURE SaveCallCreditDataAssociateLinks
GO

IF TYPE_ID('CallCreditDataAssociateLinksList') IS NOT NULL
	DROP TYPE CallCreditDataAssociateLinksList
GO

CREATE TYPE CallCreditDataAssociateLinksList AS TABLE (
	[CallCreditDataID] BIGINT NULL,
	[DeclaredAddress] BIT NULL,
	[OiaID] INT NULL,
	[NavLinkID] NVARCHAR(38) NULL,
	[AssociateName] NVARCHAR(164) NULL,
	[CreationDate] DATETIME NULL,
	[LastConfDate] DATETIME NULL,
	[SupplierName] NVARCHAR(60) NULL,
	[SupplierTypeCode] NVARCHAR(10) NULL
)
GO

CREATE PROCEDURE SaveCallCreditDataAssociateLinks
@Tbl CallCreditDataAssociateLinksList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @CallCreditDataAssociateLinksId BIGINT
	DECLARE @c INT

	SELECT @c = COUNT(*) FROM @Tbl

	IF @c = 0
		RAISERROR('Invalid argument: no/too much data to insert into SaveCallCreditDataAssociateLinks table.', 11, 1)

	INSERT INTO CallCreditDataAssociateLinks (
		[CallCreditDataID],
		[DeclaredAddress],
		[OiaID],
		[NavLinkID],
		[AssociateName],
		[CreationDate],
		[LastConfDate],
		[SupplierName],
		[SupplierTypeCode]
	) SELECT
		[CallCreditDataID],
		[DeclaredAddress],
		[OiaID],
		[NavLinkID],
		[AssociateName],
		[CreationDate],
		[LastConfDate],
		[SupplierName],
		[SupplierTypeCode]
	FROM @Tbl

	SET @CallCreditDataAssociateLinksId = SCOPE_IDENTITY()

	SELECT @CallCreditDataAssociateLinksId AS CallCreditDataAssociateLinksId
END
GO


