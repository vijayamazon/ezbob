SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LogicalGlueSaveMissingColumn') IS NOT NULL
	DROP PROCEDURE LogicalGlueSaveMissingColumn
GO

IF TYPE_ID('LogicalGlueMissingColumnList') IS NOT NULL
	DROP TYPE LogicalGlueMissingColumnList
GO

CREATE TYPE LogicalGlueMissingColumnList AS TABLE (
	[ModelOutputID] BIGINT NOT NULL,
	[ColumnName] NVARCHAR(255) NULL
)
GO

CREATE PROCEDURE LogicalGlueSaveMissingColumn
@Tbl LogicalGlueMissingColumnList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO MissingColumn (
		[ModelOutputID] ,
		[ColumnName]
	) SELECT
		[ModelOutputID] ,
		[ColumnName]
	FROM
		@Tbl
END
GO
