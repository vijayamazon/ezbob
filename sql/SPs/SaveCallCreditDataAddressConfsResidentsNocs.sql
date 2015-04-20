SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditDataAddressConfsResidentsNocs') IS NOT NULL
	DROP PROCEDURE SaveCallCreditDataAddressConfsResidentsNocs
GO

IF TYPE_ID('CallCreditDataAddressConfsResidentsNocsList') IS NOT NULL
	DROP TYPE CallCreditDataAddressConfsResidentsNocsList
GO

CREATE TYPE CallCreditDataAddressConfsResidentsNocsList AS TABLE (
	[CallCreditDataAddressConfsResidentsID] BIGINT NULL,
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

CREATE PROCEDURE SaveCallCreditDataAddressConfsResidentsNocs
@Tbl CallCreditDataAddressConfsResidentsNocsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @c INT

	SELECT @c = COUNT(*) FROM @Tbl

	IF @c = 0
		RAISERROR('Invalid argument: no/too much data to insert into SaveCallCreditDataAddressConfsResidentsNocs table.', 11, 1)

	INSERT INTO CallCreditDataAddressConfsResidentsNocs (
		[CallCreditDataAddressConfsResidentsID],
		[NoticeType],
		[RefNum],
		[DateRaised],
		[Text],
		[NameDetails],
		[CurrentAddress],
		[UnDeclaredAddressType],
		[AddressValue]
	) SELECT
		[CallCreditDataAddressConfsResidentsID],
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


