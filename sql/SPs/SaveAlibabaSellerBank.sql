SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveAlibabaSellerBank') IS NOT NULL
	DROP PROCEDURE SaveAlibabaSellerBank
GO

IF TYPE_ID('AlibabaSellerBankList') IS NOT NULL
	DROP TYPE AlibabaSellerBankList
GO

CREATE TYPE AlibabaSellerBankList AS TABLE (
	[SellerID] BIGINT NOT NULL,
	[BeneficiaryBank] NVARCHAR(100) NULL,
	[StreetAddr1] NVARCHAR(100) NULL,
	[StreetAddr2] NVARCHAR(100) NULL,
	[City] NVARCHAR(100) NULL,
	[State] NVARCHAR(100) NULL,
	[Country] NVARCHAR(100) NULL,
	[PostalCode] NVARCHAR(100) NULL,
	[SwiftCode] NVARCHAR(100) NULL,
	[AccountNumber] INT NULL,
	[WireInstructions] NVARCHAR(100) NULL
)
GO

CREATE PROCEDURE SaveAlibabaSellerBank
@Tbl AlibabaSellerBankList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO AlibabaSellerBank (
		[SellerID],
		[BeneficiaryBank],
		[StreetAddr1],
		[StreetAddr2],
		[City],
		[State],
		[Country],
		[PostalCode],
		[SwiftCode],
		[AccountNumber],
		[WireInstructions]
	) SELECT
		[SellerID],
		[BeneficiaryBank],
		[StreetAddr1],
		[StreetAddr2],
		[City],
		[State],
		[Country],
		[PostalCode],
		[SwiftCode],
		[AccountNumber],
		[WireInstructions]
	FROM @Tbl
END
GO


