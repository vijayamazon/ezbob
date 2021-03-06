SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditDataLinkAddresses') IS NOT NULL
	DROP PROCEDURE SaveCallCreditDataLinkAddresses
GO

IF TYPE_ID('CallCreditDataLinkAddressesList') IS NOT NULL
	DROP TYPE CallCreditDataLinkAddressesList
GO

CREATE TYPE CallCreditDataLinkAddressesList AS TABLE (
	[CallCreditDataID] BIGINT NULL,
	[AddressID] INT NULL,
	[Declared] BIT NULL,
	[NavLinkID] NVARCHAR(38) NULL,
	[CurrentAddress] BIT NULL,
	[UnDeclaredAddressType] INT NULL,
	[AddressValue] NVARCHAR(440) NULL
)
GO

CREATE PROCEDURE SaveCallCreditDataLinkAddresses
@Tbl CallCreditDataLinkAddressesList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @c INT

	SELECT @c = COUNT(*) FROM @Tbl

	IF @c = 0
		RAISERROR('Invalid argument: no/too much data to insert into SaveCallCreditDataLinkAddresses table.', 11, 1)

	INSERT INTO CallCreditDataLinkAddresses (
		[CallCreditDataID],
		[AddressID],
		[Declared],
		[NavLinkID],
		[CurrentAddress],
		[UnDeclaredAddressType],
		[AddressValue]
	) SELECT
		[CallCreditDataID],
		[AddressID],
		[Declared],
		[NavLinkID],
		[CurrentAddress],
		[UnDeclaredAddressType],
		[AddressValue]
	FROM @Tbl
END
GO


