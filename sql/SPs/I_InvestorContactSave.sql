SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('I_InvestorContactSave') IS NOT NULL
	DROP PROCEDURE I_InvestorContactSave
GO

IF TYPE_ID('I_InvestorContactList') IS NOT NULL
	DROP TYPE I_InvestorContactList
GO

CREATE TYPE I_InvestorContactList AS TABLE (
	[InvestorContactID] INT NOT NULL,
	[InvestorID] INT NOT NULL,
	[PersonalName] NVARCHAR(255) NULL,
	[LastName] NVARCHAR(255) NULL,
	[Email] NVARCHAR(255) NULL,
	[Role] NVARCHAR(255) NULL,
	[Comment] NVARCHAR(255) NULL,
	[IsPrimary] BIT NOT NULL,
	[Mobile] NVARCHAR(30) NULL,
	[OfficePhone] NVARCHAR(30) NULL,
	[IsActive] BIT NOT NULL,
	[Timestamp] DATETIME NOT NULL
)
GO

CREATE PROCEDURE I_InvestorContactSave
@Tbl I_InvestorContactList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO I_InvestorContact (
		[InvestorContactID],
		[InvestorID],
		[PersonalName],
		[LastName],
		[Email],
		[Role],
		[Comment],
		[IsPrimary],
		[Mobile],
		[OfficePhone],
		[IsActive],
		[Timestamp]
	) SELECT
		[InvestorContactID],
		[InvestorID],
		[PersonalName],
		[LastName],
		[Email],
		[Role],
		[Comment],
		[IsPrimary],
		[Mobile],
		[OfficePhone],
		[IsActive],
		[Timestamp]
	FROM @Tbl

	SELECT count(*) AS AddedRows FROM @Tbl
END
GO

