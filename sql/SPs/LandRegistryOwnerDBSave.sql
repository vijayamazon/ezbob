SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LandRegistryOwnerDBSave') IS NOT NULL
	DROP PROCEDURE LandRegistryOwnerDBSave
GO

IF TYPE_ID('LandRegistryOwnerDBList') IS NOT NULL
	DROP TYPE LandRegistryOwnerDBList
GO

CREATE TYPE LandRegistryOwnerDBList AS TABLE (
	[LandRegistryId] INT NOT NULL,
	[FirstName] NVARCHAR(100) NULL,
	[LastName] NVARCHAR(100) NULL,
	[CompanyName] NVARCHAR(100) NULL,
	[CompanyRegistrationNumber] NVARCHAR(100) NULL
)
GO

CREATE PROCEDURE LandRegistryOwnerDBSave
@Tbl LandRegistryOwnerDBList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO LandRegistryOwnerDB (
		[LandRegistryId],
		[FirstName],
		[LastName],
		[CompanyName],
		[CompanyRegistrationNumber]
	) SELECT
		[LandRegistryId],
		[FirstName],
		[LastName],
		[CompanyName],
		[CompanyRegistrationNumber]
	FROM @Tbl
END
GO


