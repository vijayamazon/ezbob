SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditDataCifasFilingNocs') IS NOT NULL
	DROP PROCEDURE SaveCallCreditDataCifasFilingNocs
GO

IF TYPE_ID('CallCreditDataCifasFilingNocsList') IS NOT NULL
	DROP TYPE CallCreditDataCifasFilingNocsList
GO

CREATE TYPE CallCreditDataCifasFilingNocsList AS TABLE (
	[CallCreditDataCifasFilingID] BIGINT NULL,
	[NoticeType] NVARCHAR(10) NULL,
	[Refnum] NVARCHAR(30) NULL,
	[DateRaised] DATETIME NULL,
	[Text] NVARCHAR(MAX) NULL,
	[NameDetails] NVARCHAR(164) NULL,
	[CurrentAddress] BIT NULL,
	[UnDeclaredAddressType] INT NULL,
	[AddressValue] NVARCHAR(440) NULL
)
GO

CREATE PROCEDURE SaveCallCreditDataCifasFilingNocs
@Tbl CallCreditDataCifasFilingNocsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @c INT

	SELECT @c = COUNT(*) FROM @Tbl

	IF @c = 0
		RAISERROR('Invalid argument: no/too much data to insert into SaveCallCreditDataCifasFilingNocs table.', 11, 1)

	INSERT INTO CallCreditDataCifasFilingNocs (
		[CallCreditDataCifasFilingID],
		[NoticeType],
		[Refnum],
		[DateRaised],
		[Text],
		[NameDetails],
		[CurrentAddress],
		[UnDeclaredAddressType],
		[AddressValue]
	) SELECT
		[CallCreditDataCifasFilingID],
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


