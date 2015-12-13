SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('I_ProductSubTypeSave') IS NOT NULL
	DROP PROCEDURE I_ProductSubTypeSave
GO

IF TYPE_ID('I_ProductSubTypeList') IS NOT NULL
	DROP TYPE I_ProductSubTypeList
GO

CREATE TYPE I_ProductSubTypeList AS TABLE (
	[ProductTypeID] INT NOT NULL,
	[FundingTypeID] INT NOT NULL,
	[Name] NVARCHAR(255) NULL,
	[Timestamp] DATETIME NOT NULL
)
GO

CREATE PROCEDURE I_ProductSubTypeSave
@Tbl I_ProductSubTypeList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO I_ProductSubType (
		[ProductTypeID],
		[FundingTypeID],
		[Name],
		[Timestamp]
	) SELECT
		[ProductTypeID],
		[FundingTypeID],
		[Name],
		[Timestamp]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


