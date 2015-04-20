SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditDataCifasFiling') IS NOT NULL
	DROP PROCEDURE SaveCallCreditDataCifasFiling
GO

IF TYPE_ID('CallCreditDataCifasFilingList') IS NOT NULL
	DROP TYPE CallCreditDataCifasFilingList
GO

CREATE TYPE CallCreditDataCifasFilingList AS TABLE (
	[CallCreditDataID] BIGINT NULL,
	[PersonName] NVARCHAR(164) NULL,
	[Dob] DATETIME NULL,
	[CurrentAddressP] BIT NULL,
	[UnDeclaredAddressTypeP] INT NULL,
	[AddressValueP] NVARCHAR(440) NULL,
	[CompanyNumber] NVARCHAR(8) NULL,
	[CompanyName] NVARCHAR(70) NULL,
	[CurrentAddressC] BIT NULL,
	[UnDeclaredAddressTypeC] INT NULL,
	[AddressValueC] NVARCHAR(440) NULL,
	[MemberNumber] INT NULL,
	[CaseReferenceNo] NVARCHAR(6) NULL,
	[MemberName] NVARCHAR(100) NULL,
	[ProductCode] NVARCHAR(10) NULL,
	[FraudCategory] NVARCHAR(10) NULL,
	[ProductDesc] NVARCHAR(150) NULL,
	[FraudDesc] NVARCHAR(50) NULL,
	[InputDate] DATETIME NULL,
	[ExpiryDate] DATETIME NULL,
	[TransactionType] NVARCHAR(10) NULL
)
GO

CREATE PROCEDURE SaveCallCreditDataCifasFiling
@Tbl CallCreditDataCifasFilingList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @CallCreditDataCifasFilingId BIGINT
	DECLARE @c INT

	SELECT @c = COUNT(*) FROM @Tbl

	IF @c = 0
		RAISERROR('Invalid argument: no/too much data to insert into SaveCallCreditDataCifasFiling table.', 11, 1)

	INSERT INTO CallCreditDataCifasFiling (
		[CallCreditDataID],
		[PersonName],
		[Dob],
		[CurrentAddressP],
		[UnDeclaredAddressTypeP],
		[AddressValueP],
		[CompanyNumber],
		[CompanyName],
		[CurrentAddressC],
		[UnDeclaredAddressTypeC],
		[AddressValueC],
		[MemberNumber],
		[CaseReferenceNo],
		[MemberName],
		[ProductCode],
		[FraudCategory],
		[ProductDesc],
		[FraudDesc],
		[InputDate],
		[ExpiryDate],
		[TransactionType]
	) SELECT
		[CallCreditDataID],
		[PersonName],
		[Dob],
		[CurrentAddressP],
		[UnDeclaredAddressTypeP],
		[AddressValueP],
		[CompanyNumber],
		[CompanyName],
		[CurrentAddressC],
		[UnDeclaredAddressTypeC],
		[AddressValueC],
		[MemberNumber],
		[CaseReferenceNo],
		[MemberName],
		[ProductCode],
		[FraudCategory],
		[ProductDesc],
		[FraudDesc],
		[InputDate],
		[ExpiryDate],
		[TransactionType]
	FROM @Tbl

	SET @CallCreditDataCifasFilingId = SCOPE_IDENTITY()

	SELECT @CallCreditDataCifasFilingId AS CallCreditDataCifasFilingId
END
GO


