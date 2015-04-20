SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditDataAddressConfsResidents') IS NOT NULL
	DROP PROCEDURE SaveCallCreditDataAddressConfsResidents
GO

IF TYPE_ID('CallCreditDataAddressConfsResidentsList') IS NOT NULL
	DROP TYPE CallCreditDataAddressConfsResidentsList
GO

CREATE TYPE CallCreditDataAddressConfsResidentsList AS TABLE (
	[CallCreditDataAddressConfsID] BIGINT NULL,
	[MatchType] NVARCHAR(10) NULL,
	[CurrentName] BIT NULL,
	[DeclaredAlias] BIT NULL,
	[NameDetails] NVARCHAR(164) NULL,
	[Duration] NVARCHAR(30) NULL,
	[StartDate] DATETIME NULL,
	[EndDate] DATETIME NULL,
	[ErValid] INT NULL
)
GO

CREATE PROCEDURE SaveCallCreditDataAddressConfsResidents
@Tbl CallCreditDataAddressConfsResidentsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @CallCreditDataAddressConfsResidentsId BIGINT
	DECLARE @c INT

	SELECT @c = COUNT(*) FROM @Tbl

	IF @c = 0
		RAISERROR('Invalid argument: no/too much data to insert into SaveCallCreditDataAddressConfsResidents table.', 11, 1)

	INSERT INTO CallCreditDataAddressConfsResidents (
		[CallCreditDataAddressConfsID],
		[MatchType],
		[CurrentName],
		[DeclaredAlias],
		[NameDetails],
		[Duration],
		[StartDate],
		[EndDate],
		[ErValid]
	) SELECT
		[CallCreditDataAddressConfsID],
		[MatchType],
		[CurrentName],
		[DeclaredAlias],
		[NameDetails],
		[Duration],
		[StartDate],
		[EndDate],
		[ErValid]
	FROM @Tbl

	SET @CallCreditDataAddressConfsResidentsId = SCOPE_IDENTITY()

	SELECT @CallCreditDataAddressConfsResidentsId AS CallCreditDataAddressConfsResidentsId
END
GO


