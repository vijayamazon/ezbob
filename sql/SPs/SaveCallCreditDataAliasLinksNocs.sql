SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditDataAliasLinksNocs') IS NOT NULL
	DROP PROCEDURE SaveCallCreditDataAliasLinksNocs
GO

IF TYPE_ID('CallCreditDataAliasLinksNocsList') IS NOT NULL
	DROP TYPE CallCreditDataAliasLinksNocsList
GO

CREATE TYPE CallCreditDataAliasLinksNocsList AS TABLE (
	[CallCreditDataAliasLinksID] BIGINT NULL,
	[NoticeType] NVARCHAR(10) NULL,
	[RefNum] NVARCHAR(30) NULL,
	[DateRaised] DATETIME NULL,
	[Text] NVARCHAR(4000) NULL,
	[NameDetails] NVARCHAR(164) NULL,
	[CurrentAddress] BIT NULL,
	[UnDeclaredAddressType] INT NULL,
	[AddressValue] NVARCHAR(440) NULL
)
GO

CREATE PROCEDURE SaveCallCreditDataAliasLinksNocs
@Tbl CallCreditDataAliasLinksNocsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CallCreditDataAliasLinksNocs (
		[CallCreditDataAliasLinksID],
		[NoticeType],
		[RefNum],
		[DateRaised],
		[Text],
		[NameDetails],
		[CurrentAddress],
		[UnDeclaredAddressType],
		[AddressValue]
	) SELECT
		[CallCreditDataAliasLinksID],
		[NoticeType],
		[RefNum],
		[DateRaised],
		[Text],
		[NameDetails],
		[CurrentAddress],
		[UnDeclaredAddressType],
		[AddressValue]
	FROM @Tbl
END
GO


