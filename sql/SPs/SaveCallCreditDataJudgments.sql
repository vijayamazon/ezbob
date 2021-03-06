SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditDataJudgments') IS NOT NULL
	DROP PROCEDURE SaveCallCreditDataJudgments
GO

IF TYPE_ID('CallCreditDataJudgmentsList') IS NOT NULL
	DROP TYPE CallCreditDataJudgmentsList
GO

CREATE TYPE CallCreditDataJudgmentsList AS TABLE (
	[CallCreditDataID] BIGINT NULL,
	[NameDetails] NVARCHAR(164) NULL,
	[Dob] DATETIME NULL,
	[CourtName] NVARCHAR(50) NULL,
	[CourtType] INT NULL,
	[CaseNumber] NVARCHAR(30) NULL,
	[Status] NVARCHAR(10) NULL,
	[Amount] INT NULL,
	[JudgmentDate] DATETIME NULL,
	[DateSatisfied] DATETIME NULL,
	[CurrentAddress] BIT NULL,
	[UnDeclaredAddressType] INT NULL,
	[AddressValue] NVARCHAR(440) NULL
)
GO

CREATE PROCEDURE SaveCallCreditDataJudgments
@Tbl CallCreditDataJudgmentsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @CallCreditDataJudgmentsId BIGINT
	DECLARE @c INT

	SELECT @c = COUNT(*) FROM @Tbl

	IF @c = 0
		RAISERROR('Invalid argument: no/too much data to insert into SaveCallCreditDataJudgments table.', 11, 1)

	INSERT INTO CallCreditDataJudgments (
		[CallCreditDataID],
		[NameDetails],
		[Dob],
		[CourtName],
		[CourtType],
		[CaseNumber],
		[Status],
		[Amount],
		[JudgmentDate],
		[DateSatisfied],
		[CurrentAddress],
		[UnDeclaredAddressType],
		[AddressValue]
	) SELECT
		[CallCreditDataID],
		[NameDetails],
		[Dob],
		[CourtName],
		[CourtType],
		[CaseNumber],
		[Status],
		[Amount],
		[JudgmentDate],
		[DateSatisfied],
		[CurrentAddress],
		[UnDeclaredAddressType],
		[AddressValue]
	FROM @Tbl

	SET @CallCreditDataJudgmentsId = SCOPE_IDENTITY()

	SELECT @CallCreditDataJudgmentsId AS CallCreditDataJudgmentsId
END
GO


