SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditDataAddressLinksNocs') IS NOT NULL
	DROP PROCEDURE SaveCallCreditDataAddressLinksNocs
GO

IF TYPE_ID('CallCreditDataAddressLinksNocsList') IS NOT NULL
	DROP TYPE CallCreditDataAddressLinksNocsList
GO

CREATE TYPE CallCreditDataAddressLinksNocsList AS TABLE (
	[CallCreditDataAddressLinksID] BIGINT NULL,
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

CREATE PROCEDURE SaveCallCreditDataAddressLinksNocs
@Tbl CallCreditDataAddressLinksNocsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CallCreditDataAddressLinksNocs (
		[CallCreditDataAddressLinksID],
		[NoticeType],
		[RefNum],
		[DateRaised],
		[Text],
		[NameDetails],
		[CurrentAddress],
		[UnDeclaredAddressType],
		[AddressValue]
	) SELECT
		[CallCreditDataAddressLinksID],
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


