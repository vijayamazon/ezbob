IF OBJECT_ID('ConfigTable_Refill') IS NULL
	EXECUTE('CREATE PROCEDURE ConfigTable_Refill AS SELECT 1')
GO

ALTER PROCEDURE ConfigTable_Refill
(@TheList IntIntDecimalList READONLY,
@TableName VARCHAR(100))
AS
BEGIN
	SET NOCOUNT ON
	EXECUTE('DELETE FROM ' + @TableName)
	DECLARE @v1 INT, @v2 INT, @v3 DECIMAL(18, 6)

	
	
	DECLARE cur CURSOR FOR SELECT Value1, Value2, Value3 FROM @TheList
	OPEN cur
	FETCH NEXT FROM cur INTO @v1, @v2, @v3
	WHILE @@FETCH_STATUS = 0
	BEGIN
		DECLARE @InsertStatement VARCHAR(MAX)
		SET @InsertStatement = 'INSERT INTO ' + @TableName + ' (Start, [End], Value) VALUES (' + CONVERT(NVARCHAR, @v1) + ', ' + CONVERT(NVARCHAR, @v2) + ', ' + CONVERT(NVARCHAR, @v3) + ')'
		EXECUTE(@InsertStatement)
		
		FETCH NEXT FROM cur INTO @v1, @v2, @v3
	END
	CLOSE cur
	DEALLOCATE cur
END
GO
