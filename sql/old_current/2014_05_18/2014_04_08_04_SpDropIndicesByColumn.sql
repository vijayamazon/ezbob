IF OBJECT_ID('SpDropIndicesByColumn') IS NULL
	EXECUTE('CREATE PROCEDURE SpDropIndicesByColumn AS SELECT 1')
GO

ALTER PROCEDURE SpDropIndicesByColumn
@TableName sysname,
@ColumnName sysname
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE cur CURSOR FOR
		SELECT DISTINCT
		     ind.name
		FROM
		     sys.indexes ind
			INNER JOIN sys.index_columns ic ON  ind.object_id = ic.object_id AND ind.index_id = ic.index_id
			INNER JOIN sys.columns col ON ic.object_id = col.object_id AND ic.column_id = col.column_id
			INNER JOIN sys.tables t ON ind.object_id = t.object_id
		WHERE
		     ind.is_primary_key = 0
		     AND ind.is_unique = 0
		     AND ind.is_unique_constraint = 0
		     AND t.is_ms_shipped = 0
		     AND t.object_id = OBJECT_ID(@TableName)
		     AND col.name = @ColumnName

	DECLARE @IdxName sysname

	OPEN cur

	FETCH cur INTO @IdxName

	WHILE @@FETCH_STATUS = 0
	BEGIN
		PRINT 'Dropping index ' + @TableName + '.' + @IdxName

		EXECUTE('DROP INDEX ' + @IdxName + ' ON ' + @TableName)

		FETCH cur INTO @IdxName
	END

	CLOSE cur
	DEALLOCATE cur
END
GO
