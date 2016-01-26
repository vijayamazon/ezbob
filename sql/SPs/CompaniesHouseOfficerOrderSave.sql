SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('CompaniesHouseOfficerOrderSave') IS NOT NULL
	DROP PROCEDURE CompaniesHouseOfficerOrderSave
GO

IF TYPE_ID('CompaniesHouseOfficerOrderList') IS NOT NULL
	DROP TYPE CompaniesHouseOfficerOrderList
GO

CREATE TYPE CompaniesHouseOfficerOrderList AS TABLE (
	[CompanyRefNum] NVARCHAR(255) NULL,
	[Timestamp] DATETIME NOT NULL,
	[ActiveCount] INT NOT NULL,
	[Etag] NVARCHAR(255) NULL,
	[ItemsPerPage] INT NOT NULL,
	[Kind] NVARCHAR(255) NULL,
	[Link] NVARCHAR(255) NULL,
	[ResignedCount] INT NOT NULL,
	[StartIndex] INT NOT NULL,
	[TotalResults] INT NOT NULL,
	[Error] NVARCHAR(255) NULL
)
GO

CREATE PROCEDURE CompaniesHouseOfficerOrderSave
@Tbl CompaniesHouseOfficerOrderList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CompaniesHouseOfficerOrder (
		[CompanyRefNum],
		[Timestamp],
		[ActiveCount],
		[Etag],
		[ItemsPerPage],
		[Kind],
		[Link],
		[ResignedCount],
		[StartIndex],
		[TotalResults],
		[Error]
	) SELECT
		[CompanyRefNum],
		[Timestamp],
		[ActiveCount],
		[Etag],
		[ItemsPerPage],
		[Kind],
		[Link],
		[ResignedCount],
		[StartIndex],
		[TotalResults],
		[Error]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


