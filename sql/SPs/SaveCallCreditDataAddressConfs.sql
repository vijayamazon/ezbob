SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditDataAddressConfs') IS NOT NULL
	DROP PROCEDURE SaveCallCreditDataAddressConfs
GO

IF TYPE_ID('CallCreditDataAddressConfsList') IS NOT NULL
	DROP TYPE CallCreditDataAddressConfsList
GO

CREATE TYPE CallCreditDataAddressConfsList AS TABLE (
	[CallCreditDataID] BIGINT NULL,
	[PafValid] BIT NULL,
	[OtherResidents] BIT NULL,
	[CurrentAddress] BIT NULL,
	[UnDeclaredAddressType] INT NULL,
	[AddressValue] NVARCHAR(440) NULL
)
GO

CREATE PROCEDURE SaveCallCreditDataAddressConfs
@Tbl CallCreditDataAddressConfsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CallCreditDataAddressConfs (
		[CallCreditDataID],
		[PafValid],
		[OtherResidents],
		[CurrentAddress],
		[UnDeclaredAddressType],
		[AddressValue]
	) SELECT
		[CallCreditDataID],
		[PafValid],
		[OtherResidents],
		[CurrentAddress],
		[UnDeclaredAddressType],
		[AddressValue]
	FROM @Tbl
END
GO


