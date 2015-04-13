SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditDataSearches') IS NOT NULL
	DROP PROCEDURE SaveCallCreditDataSearches
GO

IF TYPE_ID('CallCreditDataSearchesList') IS NOT NULL
	DROP TYPE CallCreditDataSearchesList
GO

CREATE TYPE CallCreditDataSearchesList AS TABLE (
	[CallCreditDataID] BIGINT NULL,
	[SearchRef] NVARCHAR(38) NULL,
	[SearchOrgType] NVARCHAR(10) NULL,
	[SearchOrgName] NVARCHAR(50) NULL,
	[YourReference] NVARCHAR(50) NULL,
	[SearchUnitName] NVARCHAR(50) NULL,
	[OwnSearch] BIT NULL,
	[SubsequentEnquiry] BIT NULL,
	[UserName] NVARCHAR(50) NULL,
	[SearchPurpose] NVARCHAR(10) NULL,
	[CreditType] NVARCHAR(10) NULL,
	[Balance] INT NULL,
	[Term] INT NULL,
	[JointApplication] BIT NULL,
	[SearchDate] DATETIME NULL,
	[NameDetailes] NVARCHAR(164) NULL,
	[Dob] DATETIME NULL,
	[StartDate] DATETIME NULL,
	[EndDate] DATETIME NULL,
	[TpOptOut] BIT NULL,
	[Transient] BIT NULL,
	[LinkType] INT NULL,
	[CurrentAddress] BIT NULL,
	[UnDeclaredAddressType] INT NULL,
	[AddressValue] NVARCHAR(440) NULL
)
GO

CREATE PROCEDURE SaveCallCreditDataSearches
@Tbl CallCreditDataSearchesList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CallCreditDataSearches (
		[CallCreditDataID],
		[SearchRef],
		[SearchOrgType],
		[SearchOrgName],
		[YourReference],
		[SearchUnitName],
		[OwnSearch],
		[SubsequentEnquiry],
		[UserName],
		[SearchPurpose],
		[CreditType],
		[Balance],
		[Term],
		[JointApplication],
		[SearchDate],
		[NameDetailes],
		[Dob],
		[StartDate],
		[EndDate],
		[TpOptOut],
		[Transient],
		[LinkType],
		[CurrentAddress],
		[UnDeclaredAddressType],
		[AddressValue]
	) SELECT
		[CallCreditDataID],
		[SearchRef],
		[SearchOrgType],
		[SearchOrgName],
		[YourReference],
		[SearchUnitName],
		[OwnSearch],
		[SubsequentEnquiry],
		[UserName],
		[SearchPurpose],
		[CreditType],
		[Balance],
		[Term],
		[JointApplication],
		[SearchDate],
		[NameDetailes],
		[Dob],
		[StartDate],
		[EndDate],
		[TpOptOut],
		[Transient],
		[LinkType],
		[CurrentAddress],
		[UnDeclaredAddressType],
		[AddressValue]
	FROM @Tbl
END
GO


