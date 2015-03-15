SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadMatrix') IS NULL
	EXECUTE('CREATE PROCEDURE LoadMatrix AS SELECT 1')
GO

ALTER PROCEDURE LoadMatrix
@MatrixName NVARCHAR(64)
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @MatrixID BIGINT

	------------------------------------------------------------------------------

	SELECT
		@MatrixID = MatrixID
	FROM
		Matrices
	WHERE
		MatrixName = @MatrixName

	------------------------------------------------------------------------------

	SELECT
		RowType = 'MetaData',
		MatrixID,
		MinRowTitleValue,
		MinColumnTitleValue
	FROM
		Matrices
	WHERE
		MatrixID = @MatrixID

	------------------------------------------------------------------------------

	SELECT
		RowType = 'Value',
		RowTitle = r.TitleValue,
		ColumnTitle = c.TitleValue,
		c.CellValue
	FROM
		MatrixRowTitles r
		INNER JOIN MatrixColumns c ON r.MatrixRowID = c.MatrixRowID
	WHERE
		r.MatrixID = @MatrixID
	ORDER BY
		CASE WHEN r.TitleValue IS NOT NULL THEN 0 ELSE 1 END,
		r.TitleValue,
		CASE WHEN c.TitleValue IS NOT NULL THEN 0 ELSE 1 END,
		c.TitleValue
END
GO
