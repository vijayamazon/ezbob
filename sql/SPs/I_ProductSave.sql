SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('I_ProductSave') IS NOT NULL
	DROP PROCEDURE I_ProductSave
GO

IF TYPE_ID('I_ProductList') IS NOT NULL
	DROP TYPE I_ProductList
GO

CREATE TYPE I_ProductList AS TABLE (
	[ProductID] INT NOT NULL,
	[Name] NVARCHAR(255) NULL,
	[IsDefault] BIT NOT NULL,
	[IsEnabled] BIT NOT NULL
)
GO

CREATE PROCEDURE I_ProductSave
@Tbl I_ProductList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO I_Product (
		[ProductID],
		[Name],
		[IsDefault],
		[IsEnabled]
	) SELECT
		[ProductID],
		[Name],
		[IsDefault],
		[IsEnabled]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


