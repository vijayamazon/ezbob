SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditDataJudgmentsNocs') IS NOT NULL
	DROP PROCEDURE SaveCallCreditDataJudgmentsNocs
GO

IF TYPE_ID('CallCreditDataJudgmentsNocsList') IS NOT NULL
	DROP TYPE CallCreditDataJudgmentsNocsList
GO

CREATE TYPE CallCreditDataJudgmentsNocsList AS TABLE (
	[CallCreditDataJudgmentsID] BIGINT NULL,
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

CREATE PROCEDURE SaveCallCreditDataJudgmentsNocs
@Tbl CallCreditDataJudgmentsNocsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CallCreditDataJudgmentsNocs (
		[CallCreditDataJudgmentsID],
		[NoticeType],
		[RefNum],
		[DateRaised],
		[Text],
		[NameDetails],
		[CurrentAddress],
		[UnDeclaredAddressType],
		[AddressValue]
	) SELECT
		[CallCreditDataJudgmentsID],
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


