SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditDataBais') IS NOT NULL
	DROP PROCEDURE SaveCallCreditDataBais
GO

IF TYPE_ID('CallCreditDataBaisList') IS NOT NULL
	DROP TYPE CallCreditDataBaisList
GO

CREATE TYPE CallCreditDataBaisList AS TABLE (
	[CallCreditDataID] BIGINT NULL,
	[DischargeDate] DATETIME NULL,
	[LineOfBusiness] NVARCHAR(30) NULL,
	[CourtName] NVARCHAR(50) NULL,
	[CurrentStatus] NVARCHAR(10) NULL,
	[Amount] INT NULL,
	[OrderType] NVARCHAR(10) NULL,
	[OrderDate] DATETIME NULL,
	[CaseYear] INT NULL,
	[CaseRef] NVARCHAR(20) NULL,
	[NameDetails] NVARCHAR(164) NULL,
	[TradingName] NVARCHAR(60) NULL,
	[Dob] DATETIME NULL,
	[CurrentAddress] BIT NULL,
	[UnDeclaredAddressType] INT NULL,
	[AddressValue] NVARCHAR(440) NULL,
	[RestrictionType] NVARCHAR(10) NULL,
	[Startdate] DATETIME NULL,
	[Enddate] DATETIME NULL
)
GO

CREATE PROCEDURE SaveCallCreditDataBais
@Tbl CallCreditDataBaisList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CallCreditDataBais (
		[CallCreditDataID],
		[DischargeDate],
		[LineOfBusiness],
		[CourtName],
		[CurrentStatus],
		[Amount],
		[OrderType],
		[OrderDate],
		[CaseYear],
		[CaseRef],
		[NameDetails],
		[TradingName],
		[Dob],
		[CurrentAddress],
		[UnDeclaredAddressType],
		[AddressValue],
		[RestrictionType],
		[Startdate],
		[Enddate]
	) SELECT
		[CallCreditDataID],
		[DischargeDate],
		[LineOfBusiness],
		[CourtName],
		[CurrentStatus],
		[Amount],
		[OrderType],
		[OrderDate],
		[CaseYear],
		[CaseRef],
		[NameDetails],
		[TradingName],
		[Dob],
		[CurrentAddress],
		[UnDeclaredAddressType],
		[AddressValue],
		[RestrictionType],
		[Startdate],
		[Enddate]
	FROM @Tbl
END
GO


