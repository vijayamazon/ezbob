SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LogicalGlueSaveEncodingFailure') IS NOT NULL
	DROP PROCEDURE LogicalGlueSaveEncodingFailure
GO

IF TYPE_ID('LogicalGlueEncodingFailureList') IS NOT NULL
	DROP TYPE LogicalGlueEncodingFailureList
GO

CREATE TYPE LogicalGlueEncodingFailureList AS TABLE (
	[RowIndex] INT NOT NULL,
	[ColumnName] NVARCHAR(255) NULL,
	[UnencodedValue] NVARCHAR(MAX) NULL,
	[Reason] NVARCHAR(MAX) NULL,
	[Message] NVARCHAR(MAX) NULL,
	[ModelOutputID] BIGINT NOT NULL
)
GO

CREATE PROCEDURE LogicalGlueSaveEncodingFailure
@Tbl LogicalGlueEncodingFailureList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO LogicalGlueModelEncodingFailures (
		[ModelOutputID] ,
		[RowIndex],
		[ColumnName],
		[UnencodedValue],
		[Reason],
		[Message]
	) SELECT
		[ModelOutputID] ,
		[RowIndex],
		[ColumnName],
		[UnencodedValue],
		[Reason],
		[Message]
	FROM
		@Tbl
END
GO
