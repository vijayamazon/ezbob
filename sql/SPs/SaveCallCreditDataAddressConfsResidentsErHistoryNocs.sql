SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditDataAddressConfsResidentsErHistoryNocs') IS NOT NULL
	DROP PROCEDURE SaveCallCreditDataAddressConfsResidentsErHistoryNocs
GO

IF TYPE_ID('CallCreditDataAddressConfsResidentsErHistoryNocsList') IS NOT NULL
	DROP TYPE CallCreditDataAddressConfsResidentsErHistoryNocsList
GO

CREATE TYPE CallCreditDataAddressConfsResidentsErHistoryNocsList AS TABLE (
	[CallCreditDataAddressConfsResidentsErHistoryID] BIGINT NULL,
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

CREATE PROCEDURE SaveCallCreditDataAddressConfsResidentsErHistoryNocs
@Tbl CallCreditDataAddressConfsResidentsErHistoryNocsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @c INT

	SELECT @c = COUNT(*) FROM @Tbl

	IF @c = 0
		RAISERROR('Invalid argument: no/too much data to insert into SaveCallCreditDataAddressConfsResidentsErHistoryNocs table.', 11, 1)

	INSERT INTO CallCreditDataAddressConfsResidentsErHistoryNocs (
		[CallCreditDataAddressConfsResidentsErHistoryID],
		[NoticeType],
		[RefNum],
		[DateRaised],
		[Text],
		[NameDetails],
		[CurrentAddress],
		[UnDeclaredAddressType],
		[AddressValue]
	) SELECT
		[CallCreditDataAddressConfsResidentsErHistoryID],
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


