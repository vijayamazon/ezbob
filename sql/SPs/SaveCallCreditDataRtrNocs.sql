SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditDataRtrNocs') IS NOT NULL
	DROP PROCEDURE SaveCallCreditDataRtrNocs
GO

IF TYPE_ID('CallCreditDataRtrNocsList') IS NOT NULL
	DROP TYPE CallCreditDataRtrNocsList
GO

CREATE TYPE CallCreditDataRtrNocsList AS TABLE (
	[CallCreditDataRtrID] BIGINT NULL,
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

CREATE PROCEDURE SaveCallCreditDataRtrNocs
@Tbl CallCreditDataRtrNocsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @c INT

	SELECT @c = COUNT(*) FROM @Tbl

	IF @c = 0
		RAISERROR('Invalid argument: no/too much data to insert into SaveCallCreditDataRtrNocs table.', 11, 1)

	INSERT INTO CallCreditDataRtrNocs (
		[CallCreditDataRtrID],
		[NoticeType],
		[Refnum],
		[DateRaised],
		[Text],
		[NameDetails],
		[CurrentAddress],
		[UnDeclaredAddressType],
		[AddressValue]
	) SELECT
		[CallCreditDataRtrID],
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


