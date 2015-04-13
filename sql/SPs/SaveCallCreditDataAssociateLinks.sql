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
END
GO


