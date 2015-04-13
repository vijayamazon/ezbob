SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditDataCifasPlusCasesNocs') IS NOT NULL
	DROP PROCEDURE SaveCallCreditDataCifasPlusCasesNocs
GO

IF TYPE_ID('CallCreditDataCifasPlusCasesNocsList') IS NOT NULL
	DROP TYPE CallCreditDataCifasPlusCasesNocsList
GO

CREATE TYPE CallCreditDataCifasPlusCasesNocsList AS TABLE (
	[CallCreditDataCifasPlusCasesID] BIGINT NULL,
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

CREATE PROCEDURE SaveCallCreditDataCifasPlusCasesNocs
@Tbl CallCreditDataCifasPlusCasesNocsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CallCreditDataCifasPlusCasesNocs (
		[CallCreditDataCifasPlusCasesID],
		[NoticeType],
		[Refnum],
		[DateRaised],
		[Text],
		[NameDetails],
		[CurrentAddress],
		[UnDeclaredAddressType],
		[AddressValue]
	) SELECT
		[CallCreditDataCifasPlusCasesID],
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


