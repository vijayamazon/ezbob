SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditApplicantAddresses') IS NOT NULL
	DROP PROCEDURE SaveCallCreditApplicantAddresses
GO

IF TYPE_ID('CallCreditApplicantAddressesList') IS NOT NULL
	DROP TYPE CallCreditApplicantAddressesList
GO

CREATE TYPE CallCreditApplicantAddressesList AS TABLE (
	[CallCreditID] BIGINT NULL,
	[AbodeNo] NVARCHAR(30) NULL,
	[BuildingNo] NVARCHAR(12) NULL,
	[BuildingName] NVARCHAR(50) NULL,
	[Street1] NVARCHAR(50) NULL,
	[Street2] NVARCHAR(50) NULL,
	[SubLocality] NVARCHAR(35) NULL,
	[Locality] NVARCHAR(35) NULL,
	[PostTown] NVARCHAR(25) NULL,
	[PostCode] NVARCHAR(8) NULL,
	[StartDate] DATETIME NULL,
	[EndDate] DATETIME NULL,
	[Duration] NVARCHAR(15) NULL
)
GO

CREATE PROCEDURE SaveCallCreditApplicantAddresses
@Tbl CallCreditApplicantAddressesList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @c INT

	SELECT @c = COUNT(*) FROM @Tbl

	IF @c = 0
		RAISERROR('Invalid argument: no/too much data to insert into SaveCallCreditApplicantAddresses table.', 11, 1)

	INSERT INTO CallCreditApplicantAddresses (
		[CallCreditID],
		[AbodeNo],
		[BuildingNo],
		[BuildingName],
		[Street1],
		[Street2],
		[SubLocality],
		[Locality],
		[PostTown],
		[PostCode],
		[StartDate],
		[EndDate],
		[Duration]
	) SELECT
		[CallCreditID],
		[AbodeNo],
		[BuildingNo],
		[BuildingName],
		[Street1],
		[Street2],
		[SubLocality],
		[Locality],
		[PostTown],
		[PostCode],
		[StartDate],
		[EndDate],
		[Duration]
	FROM @Tbl
END
GO


