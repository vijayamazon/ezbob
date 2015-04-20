SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditDataAccs') IS NOT NULL
	DROP PROCEDURE SaveCallCreditDataAccs
GO

IF TYPE_ID('CallCreditDataAccsList') IS NOT NULL
	DROP TYPE CallCreditDataAccsList
GO

CREATE TYPE CallCreditDataAccsList AS TABLE (
	[CallCreditDataID] BIGINT NULL,
	[OiaID] INT NULL,
	[AccHolderName] NVARCHAR(164) NULL,
	[Dob] DATETIME NULL,
	[StatusCode] NVARCHAR(10) NULL,
	[StartDate] DATETIME NULL,
	[EndDate] DATETIME NULL,
	[CurrentAddress] BIT NULL,
	[UnDeclaredAddressType] INT NULL,
	[AddressValue] NVARCHAR(440) NULL,
	[DefDate] DATETIME NULL,
	[OrigDefBal] INT NULL,
	[TermBal] INT NULL,
	[DefSatDate] DATETIME NULL,
	[RepoDate] DATETIME NULL,
	[DelinqDate] DATETIME NULL,
	[DelinqBal] INT NULL,
	[AccNo] NVARCHAR(20) NULL,
	[AccSuffix] INT NULL,
	[Joint] INT NULL,
	[Status] NVARCHAR(10) NULL,
	[DateUpdated] DATETIME NULL,
	[AccTypeCode] NVARCHAR(10) NULL,
	[AccGroupId] NVARCHAR(10) NULL,
	[CurrencyCode] NVARCHAR(10) NULL,
	[Balance] INT NULL,
	[CurCreditLimit] INT NULL,
	[OpenBalance] INT NULL,
	[ArrStartDate] DATETIME NULL,
	[ArrEndDate] DATETIME NULL,
	[PayStartDate] DATETIME NULL,
	[accStartDate] DATETIME NULL,
	[AccEndDate] DATETIME NULL,
	[RegPayment] INT NULL,
	[ExpectedPayment] INT NULL,
	[ActualPayment] INT NULL,
	[RepayPeriod] INT NULL,
	[RepayFreqCode] NVARCHAR(10) NULL,
	[LumpPayment] INT NULL,
	[PenIntAmt] INT NULL,
	[PromotionalRate] BIT NULL,
	[MinimumPayment] BIT NULL,
	[StatementBalance] INT NULL,
	[SupplierName] NVARCHAR(60) NULL,
	[SupplierTypeCode] NVARCHAR(10) NULL,
	[Apacs] BIT NULL
)
GO

CREATE PROCEDURE SaveCallCreditDataAccs
@Tbl CallCreditDataAccsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @CallCreditDataAccsId BIGINT
	DECLARE @c INT

	SELECT @c = COUNT(*) FROM @Tbl

	IF @c = 0
		RAISERROR('Invalid argument: no/too much data to insert into SaveCallCreditDataAccs table.', 11, 1)

	INSERT INTO CallCreditDataAccs (
		[CallCreditDataID],
		[OiaID],
		[AccHolderName],
		[Dob],
		[StatusCode],
		[StartDate],
		[EndDate],
		[CurrentAddress],
		[UnDeclaredAddressType],
		[AddressValue],
		[DefDate],
		[OrigDefBal],
		[TermBal],
		[DefSatDate],
		[RepoDate],
		[DelinqDate],
		[DelinqBal],
		[AccNo],
		[AccSuffix],
		[Joint],
		[Status],
		[DateUpdated],
		[AccTypeCode],
		[AccGroupId],
		[CurrencyCode],
		[Balance],
		[CurCreditLimit],
		[OpenBalance],
		[ArrStartDate],
		[ArrEndDate],
		[PayStartDate],
		[accStartDate],
		[AccEndDate],
		[RegPayment],
		[ExpectedPayment],
		[ActualPayment],
		[RepayPeriod],
		[RepayFreqCode],
		[LumpPayment],
		[PenIntAmt],
		[PromotionalRate],
		[MinimumPayment],
		[StatementBalance],
		[SupplierName],
		[SupplierTypeCode],
		[Apacs]
	) SELECT
		[CallCreditDataID],
		[OiaID],
		[AccHolderName],
		[Dob],
		[StatusCode],
		[StartDate],
		[EndDate],
		[CurrentAddress],
		[UnDeclaredAddressType],
		[AddressValue],
		[DefDate],
		[OrigDefBal],
		[TermBal],
		[DefSatDate],
		[RepoDate],
		[DelinqDate],
		[DelinqBal],
		[AccNo],
		[AccSuffix],
		[Joint],
		[Status],
		[DateUpdated],
		[AccTypeCode],
		[AccGroupId],
		[CurrencyCode],
		[Balance],
		[CurCreditLimit],
		[OpenBalance],
		[ArrStartDate],
		[ArrEndDate],
		[PayStartDate],
		[accStartDate],
		[AccEndDate],
		[RegPayment],
		[ExpectedPayment],
		[ActualPayment],
		[RepayPeriod],
		[RepayFreqCode],
		[LumpPayment],
		[PenIntAmt],
		[PromotionalRate],
		[MinimumPayment],
		[StatementBalance],
		[SupplierName],
		[SupplierTypeCode],
		[Apacs]
	FROM @Tbl

	SET @CallCreditDataAccsId = SCOPE_IDENTITY()

	SELECT @CallCreditDataAccsId AS CallCreditDataAccsId
END
GO


