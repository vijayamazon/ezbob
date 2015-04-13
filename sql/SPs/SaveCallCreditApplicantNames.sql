SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditApplicantNames') IS NOT NULL
	DROP PROCEDURE SaveCallCreditApplicantNames
GO

IF TYPE_ID('CallCreditApplicantNamesList') IS NOT NULL
	DROP TYPE CallCreditApplicantNamesList
GO

CREATE TYPE CallCreditApplicantNamesList AS TABLE (
	[CallCreditID] BIGINT NULL,
	[Title] NVARCHAR(30) NULL,
	[Forename] NVARCHAR(30) NULL,
	[OtherNames] NVARCHAR(40) NULL,
	[Surname] NVARCHAR(30) NULL,
	[Suffix] NVARCHAR(30) NULL
)
GO

CREATE PROCEDURE SaveCallCreditApplicantNames
@Tbl CallCreditApplicantNamesList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CallCreditApplicantNames (
		[CallCreditID],
		[Title],
		[Forename],
		[OtherNames],
		[Surname],
		[Suffix]
	) SELECT
		[CallCreditID],
		[Title],
		[Forename],
		[OtherNames],
		[Surname],
		[Suffix]
	FROM @Tbl
END
GO


