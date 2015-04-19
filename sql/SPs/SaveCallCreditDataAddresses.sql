SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditDataAddresses') IS NOT NULL
	DROP PROCEDURE SaveCallCreditDataAddresses
GO

IF TYPE_ID('CallCreditDataAddressesList') IS NOT NULL
	DROP TYPE CallCreditDataAddressesList
GO

CREATE TYPE CallCreditDataAddressesList AS TABLE (
	[CallCreditDataID] BIGINT NULL,
	[CurrentAddress] BIT NULL,
	[AddressId] INT NULL,
	[Messagecode] INT NULL,
	[UnDeclaredAddressType] INT NULL,
	[AddressValue] NVARCHAR(440) NULL
)
GO

CREATE PROCEDURE SaveCallCreditDataAddresses
@Tbl CallCreditDataAddressesList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @c INT

	SELECT @c = COUNT(*) FROM @Tbl

	IF @c = 0
		RAISERROR('Invalid argument: no/too much data to insert into SaveCallCreditDataAddresses table.', 11, 1)

	INSERT INTO CallCreditDataAddresses (
		[CallCreditDataID],
		[CurrentAddress],
		[AddressId],
		[Messagecode],
		[UnDeclaredAddressType],
		[AddressValue]
	) SELECT
		[CallCreditDataID],
		[CurrentAddress],
		[AddressId],
		[Messagecode],
		[UnDeclaredAddressType],
		[AddressValue]
	FROM @Tbl
END
GO


