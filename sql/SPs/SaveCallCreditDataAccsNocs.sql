SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditDataAccsNocs') IS NOT NULL
	DROP PROCEDURE SaveCallCreditDataAccsNocs
GO

IF TYPE_ID('CallCreditDataAccsNocsList') IS NOT NULL
	DROP TYPE CallCreditDataAccsNocsList
GO

CREATE TYPE CallCreditDataAccsNocsList AS TABLE (
	[CallCreditDataAccsID] BIGINT NULL,
	[NoticeType] NVARCHAR(10) NULL,
	[RefNum] NVARCHAR(30) NULL,
	[DateRaised] DATETIME NULL,
	[Text] NVARCHAR(MAX) NULL,
	[NameDetails] NVARCHAR(164) NULL,
	[CurrentAddress] BIT NULL,
	[UnDeclaredAddressType] INT NULL,
	[AddressValue] NVARCHAR(440) NULL
)
GO

CREATE PROCEDURE SaveCallCreditDataAccsNocs
@Tbl CallCreditDataAccsNocsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @c INT

	SELECT @c = COUNT(*) FROM @Tbl

	IF @c = 0
		RAISERROR('Invalid argument: no/too much data to insert into SaveCallCreditDataAccsNocs table.', 11, 1)

	INSERT INTO CallCreditDataAccsNocs (
		[CallCreditDataAccsID],
		[NoticeType],
		[RefNum],
		[DateRaised],
		[Text],
		[NameDetails],
		[CurrentAddress],
		[UnDeclaredAddressType],
		[AddressValue]
	) SELECT
		[CallCreditDataAccsID],
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


