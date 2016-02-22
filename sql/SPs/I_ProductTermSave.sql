SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('I_ProductTermSave') IS NOT NULL
	DROP PROCEDURE I_ProductTermSave
GO

IF TYPE_ID('I_ProductTermList') IS NOT NULL
	DROP TYPE I_ProductTermList
GO

CREATE TYPE I_ProductTermList AS TABLE (
	[Name] NVARCHAR(255) NULL,
	[FromMonths] INT NOT NULL,
	[ToMonths] INT NOT NULL,
	[Timestamp] DATETIME NOT NULL
)
GO

CREATE PROCEDURE I_ProductTermSave
@Tbl I_ProductTermList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO I_ProductTerm (
		[Name],
		[FromMonths],
		[ToMonths],
		[Timestamp]
	) SELECT
		[Name],
		[FromMonths],
		[ToMonths],
		[Timestamp]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


