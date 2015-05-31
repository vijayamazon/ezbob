SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveAlibabaSeller') IS NOT NULL
	DROP PROCEDURE SaveAlibabaSeller
GO

IF TYPE_ID('AlibabaSellerList') IS NOT NULL
	DROP TYPE AlibabaSellerList
GO

CREATE TYPE AlibabaSellerList AS TABLE (
	[ContractID] BIGINT NULL,
	[BusinessName] NVARCHAR(100) NULL,
	[AliMemberId] NVARCHAR(100) NULL,
	[Street1] NVARCHAR(100) NULL,
	[Street2] NVARCHAR(100) NULL,
	[City] NVARCHAR(100) NULL,
	[State] NVARCHAR(100) NULL,
	[Country] NVARCHAR(100) NULL,
	[PostalCode] NVARCHAR(100) NULL,
	[AuthRepFname] NVARCHAR(100) NULL,
	[AuthRepLname] NVARCHAR(100) NULL,
	[Phone] NVARCHAR(100) NULL,
	[Fax] NVARCHAR(100) NULL,
	[Email] NVARCHAR(100) NULL,
	[GoldSupplierFlag] NVARCHAR(100) NULL,
	[TenureWithAlibaba] NVARCHAR(100) NULL,
	[BusinessStartDate] DATETIME NULL,
	[Size] INT NULL,
	[suspiciousReportCountCounterfeitProduct] INT NULL,
	[suspiciousReportCountRestrictedProhibitedProduct] INT NULL,
	[suspiciousReportCountSuspiciousMember] INT NULL,
	[ResponseRate] INT NULL,
	[GoldMemberStartDate] DATETIME NULL,
	[QuotationPerformance] INT NULL
)
GO

CREATE PROCEDURE SaveAlibabaSeller
@Tbl AlibabaSellerList READONLY
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @SellerID BIGINT
	
	INSERT INTO AlibabaSeller (
		[ContractID],
		[BusinessName],
		[AliMemberId],
		[Street1],
		[Street2],
		[City],
		[State],
		[Country],
		[PostalCode],
		[AuthRepFname],
		[AuthRepLname],
		[Phone],
		[Fax],
		[Email],
		[GoldSupplierFlag],
		[TenureWithAlibaba],
		[BusinessStartDate],
		[Size],
		[suspiciousReportCountCounterfeitProduct],
		[suspiciousReportCountRestrictedProhibitedProduct],
		[suspiciousReportCountSuspiciousMember],
		[ResponseRate],
		[GoldMemberStartDate],
		[QuotationPerformance]
	) SELECT
		[ContractID],
		[BusinessName],
		[AliMemberId],
		[Street1],
		[Street2],
		[City],
		[State],
		[Country],
		[PostalCode],
		[AuthRepFname],
		[AuthRepLname],
		[Phone],
		[Fax],
		[Email],
		[GoldSupplierFlag],
		[TenureWithAlibaba],
		[BusinessStartDate],
		[Size],
		[suspiciousReportCountCounterfeitProduct],
		[suspiciousReportCountRestrictedProhibitedProduct],
		[suspiciousReportCountSuspiciousMember],
		[ResponseRate],
		[GoldMemberStartDate],
		[QuotationPerformance]
	FROM @Tbl
		
	SET @SellerID = SCOPE_IDENTITY()

	SELECT @SellerID AS SellerID
	
END
GO


