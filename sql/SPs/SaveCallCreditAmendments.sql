SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditAmendments') IS NOT NULL
	DROP PROCEDURE SaveCallCreditAmendments
GO

IF TYPE_ID('CallCreditAmendmentsList') IS NOT NULL
	DROP TYPE CallCreditAmendmentsList
GO

CREATE TYPE CallCreditAmendmentsList AS TABLE (
	[CallCreditID] BIGINT NULL,
	[AmendmentName] NVARCHAR(20) NULL,
	[AmendmentType] NVARCHAR(6) NULL,
	[Balorlim] INT NULL,
	[Term] NVARCHAR(15) NULL,
	[AbodeNo] NVARCHAR(30) NULL,
	[BuildingNo] NVARCHAR(12) NULL,
	[BuildingName] NVARCHAR(50) NULL,
	[Street1] NVARCHAR(50) NULL,
	[Street2] NVARCHAR(50) NULL,
	[Sublocality] NVARCHAR(35) NULL,
	[Locality] NVARCHAR(35) NULL,
	[PostTown] NVARCHAR(25) NULL,
	[PostCode] NVARCHAR(8) NULL,
	[StartDate] DATETIME NULL,
	[EndDate] DATETIME NULL,
	[Duration] NVARCHAR(30) NULL,
	[Title] NVARCHAR(30) NULL,
	[Forename] NVARCHAR(30) NULL,
	[OtherNames] NVARCHAR(40) NULL,
	[SurName] NVARCHAR(30) NULL,
	[Suffix] NVARCHAR(30) NULL
)
GO

CREATE PROCEDURE SaveCallCreditAmendments
@Tbl CallCreditAmendmentsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @c INT

	SELECT @c = COUNT(*) FROM @Tbl

	IF @c = 0
		RAISERROR('Invalid argument: no/too much data to insert into SaveCallCreditAmendments table.', 11, 1)

	INSERT INTO CallCreditAmendments (
		[CallCreditID],
		[AmendmentName],
		[AmendmentType],
		[Balorlim],
		[Term],
		[AbodeNo],
		[BuildingNo],
		[BuildingName],
		[Street1],
		[Street2],
		[Sublocality],
		[Locality],
		[PostTown],
		[PostCode],
		[StartDate],
		[EndDate],
		[Duration],
		[Title],
		[Forename],
		[OtherNames],
		[SurName],
		[Suffix]
	) SELECT
		[CallCreditID],
		[AmendmentName],
		[AmendmentType],
		[Balorlim],
		[Term],
		[AbodeNo],
		[BuildingNo],
		[BuildingName],
		[Street1],
		[Street2],
		[Sublocality],
		[Locality],
		[PostTown],
		[PostCode],
		[StartDate],
		[EndDate],
		[Duration],
		[Title],
		[Forename],
		[OtherNames],
		[SurName],
		[Suffix]
	FROM @Tbl
END
GO


