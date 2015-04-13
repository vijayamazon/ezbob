SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditDataNocs') IS NOT NULL
	DROP PROCEDURE SaveCallCreditDataNocs
GO

IF TYPE_ID('CallCreditDataNocsList') IS NOT NULL
	DROP TYPE CallCreditDataNocsList
GO

CREATE TYPE CallCreditDataNocsList AS TABLE (
	[CallCreditDataID] BIGINT NULL,
	[NoticeType] NVARCHAR(10) NULL,
	[Refnum] NVARCHAR(30) NULL,
	[DateRaised] DATETIME NULL,
	[Text] NVARCHAR(4000) NULL,
	[NameDetails] NVARCHAR(164) NULL,
	[CurrentAddress] BIT NULL,
	[UnDeclaredAddressType] INT NULL,
	[AddressValue] NVARCHAR(440) NULL
)
GO

CREATE PROCEDURE SaveCallCreditDataNocs
@Tbl CallCreditDataNocsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CallCreditDataNocs (
		[CallCreditDataID],
		[NoticeType],
		[Refnum],
		[DateRaised],
		[Text],
		[NameDetails],
		[CurrentAddress],
		[UnDeclaredAddressType],
		[AddressValue]
	) SELECT
		[CallCreditDataID],
		[NoticeType],
		[Refnum],
		[DateRaised],
		[Text],
		[NameDetails],
		[CurrentAddress],
		[UnDeclaredAddressType],
		[AddressValue]
	FROM @Tbl
END
GO


