SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditDataBaisNocs') IS NOT NULL
	DROP PROCEDURE SaveCallCreditDataBaisNocs
GO

IF TYPE_ID('CallCreditDataBaisNocsList') IS NOT NULL
	DROP TYPE CallCreditDataBaisNocsList
GO

CREATE TYPE CallCreditDataBaisNocsList AS TABLE (
	[CallCreditDataBaisID] BIGINT NULL,
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

CREATE PROCEDURE SaveCallCreditDataBaisNocs
@Tbl CallCreditDataBaisNocsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @c INT

	SELECT @c = COUNT(*) FROM @Tbl

	IF @c = 0
		RAISERROR('Invalid argument: no/too much data to insert into SaveCallCreditDataBaisNocs table.', 11, 1)

	INSERT INTO CallCreditDataBaisNocs (
		[CallCreditDataBaisID],
		[NoticeType],
		[RefNum],
		[DateRaised],
		[Text],
		[NameDetails],
		[CurrentAddress],
		[UnDeclaredAddressType],
		[AddressValue]
	) SELECT
		[CallCreditDataBaisID],
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


