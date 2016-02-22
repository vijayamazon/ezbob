SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('I_ProductTypeSave') IS NOT NULL
	DROP PROCEDURE I_ProductTypeSave
GO

IF TYPE_ID('I_ProductTypeList') IS NOT NULL
	DROP TYPE I_ProductTypeList
GO

CREATE TYPE I_ProductTypeList AS TABLE (
	[ProductID] INT NOT NULL,
	[Name] NVARCHAR(255) NULL,
	[Timestamp] DATETIME NOT NULL
)
GO

CREATE PROCEDURE I_ProductTypeSave
@Tbl I_ProductTypeList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO I_ProductType (
		[ProductID],
		[Name],
		[Timestamp]
	) SELECT
		[ProductID],
		[Name],
		[Timestamp]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


