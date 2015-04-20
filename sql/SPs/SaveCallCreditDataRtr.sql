SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditDataRtr') IS NOT NULL
	DROP PROCEDURE SaveCallCreditDataRtr
GO

IF TYPE_ID('CallCreditDataRtrList') IS NOT NULL
	DROP TYPE CallCreditDataRtrList
GO

CREATE TYPE CallCreditDataRtrList AS TABLE (
	[CallCreditDataID] BIGINT NULL,
	[HolderName] NVARCHAR(164) NULL,
	[Dob] DATETIME NULL,
	[CurrentAddress] BIT NULL,
	[UnDeclaredAddressType] INT NULL,
	[AddressValue] NVARCHAR(440) NULL,
	[Updated] DATETIME NULL,
	[OrgTypeCode] NVARCHAR(164) NULL,
	[OrgName] NVARCHAR(60) NULL,
	[AccNum] NVARCHAR(20) NULL,
	[AccSuffix] NVARCHAR(10) NULL,
	[AccTypeCode] NVARCHAR(10) NULL,
	[Balance] INT NULL,
	[Limit] INT NULL,
	[StartDate] DATETIME NULL,
	[EndDate] DATETIME NULL,
	[AccStatusCode] NVARCHAR(10) NULL,
	[RepayFreqCode] NVARCHAR(10) NULL,
	[NumOverdue] INT NULL,
	[Rollover] BIT NULL,
	[CrediText] BIT NULL,
	[ChangePay] BIT NULL,
	[NextPayAmount] INT NULL
)
GO

CREATE PROCEDURE SaveCallCreditDataRtr
@Tbl CallCreditDataRtrList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @CallCreditDataRtrId BIGINT
	DECLARE @c INT

	SELECT @c = COUNT(*) FROM @Tbl

	IF @c = 0
		RAISERROR('Invalid argument: no/too much data to insert into SaveCallCreditDataRtr table.', 11, 1)

	INSERT INTO CallCreditDataRtr (
		[CallCreditDataID],
		[HolderName],
		[Dob],
		[CurrentAddress],
		[UnDeclaredAddressType],
		[AddressValue],
		[Updated],
		[OrgTypeCode],
		[OrgName],
		[AccNum],
		[AccSuffix],
		[AccTypeCode],
		[Balance],
		[Limit],
		[StartDate],
		[EndDate],
		[AccStatusCode],
		[RepayFreqCode],
		[NumOverdue],
		[Rollover],
		[CrediText],
		[ChangePay],
		[NextPayAmount]
	) SELECT
		[CallCreditDataID],
		[HolderName],
		[Dob],
		[CurrentAddress],
		[UnDeclaredAddressType],
		[AddressValue],
		[Updated],
		[OrgTypeCode],
		[OrgName],
		[AccNum],
		[AccSuffix],
		[AccTypeCode],
		[Balance],
		[Limit],
		[StartDate],
		[EndDate],
		[AccStatusCode],
		[RepayFreqCode],
		[NumOverdue],
		[Rollover],
		[CrediText],
		[ChangePay],
		[NextPayAmount]
	FROM @Tbl

	SET @CallCreditDataRtrId = SCOPE_IDENTITY()

	SELECT @CallCreditDataRtrId AS CallCreditDataRtrId
END
GO


