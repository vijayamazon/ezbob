SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditDataAssociateLinksNocs') IS NOT NULL
	DROP PROCEDURE SaveCallCreditDataAssociateLinksNocs
GO

IF TYPE_ID('CallCreditDataAssociateLinksNocsList') IS NOT NULL
	DROP TYPE CallCreditDataAssociateLinksNocsList
GO

CREATE TYPE CallCreditDataAssociateLinksNocsList AS TABLE (
	[CallCreditDataAssociateLinksID] BIGINT NULL,
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

CREATE PROCEDURE SaveCallCreditDataAssociateLinksNocs
@Tbl CallCreditDataAssociateLinksNocsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @c INT

	SELECT @c = COUNT(*) FROM @Tbl

	IF @c = 0
		RAISERROR('Invalid argument: no/too much data to insert into SaveCallCreditDataAssociateLinksNocs table.', 11, 1)

	INSERT INTO CallCreditDataAssociateLinksNocs (
		[CallCreditDataAssociateLinksID],
		[NoticeType],
		[RefNum],
		[DateRaised],
		[Text],
		[NameDetails],
		[CurrentAddress],
		[UnDeclaredAddressType],
		[AddressValue]
	) SELECT
		[CallCreditDataAssociateLinksID],
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


