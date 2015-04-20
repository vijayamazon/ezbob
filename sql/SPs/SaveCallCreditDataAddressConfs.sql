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

	DECLARE @CallCreditDataAddressConfsId BIGINT
	DECLARE @c INT

	SELECT @c = COUNT(*) FROM @Tbl

	IF @c = 0
		RAISERROR('Invalid argument: no/too much data to insert into SaveCallCreditDataAddressConfs table.', 11, 1)

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

	SET @CallCreditDataAddressConfsId = SCOPE_IDENTITY()

	SELECT @CallCreditDataAddressConfsId AS CallCreditDataAddressConfsId
END
GO


