DECLARE @DefaultConstraint SYSNAME,
	@DropStmt NVARCHAR(500)

SELECT @DefaultConstraint = dc.name
	FROM sys.default_constraints dc
	INNER JOIN syscolumns c
		ON dc.parent_column_id = c.colorder
		AND dc.parent_object_id = c.id
		AND c.name = 'CurrentBalanceSum'
		AND c.id = OBJECT_ID('MP_ExperianDataCache')
		
SET @DropStmt = 'ALTER TABLE MP_ExperianDataCache DROP CONSTRAINT ' + @DefaultConstraint

EXEC(@DropStmt)
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'CurrentBalanceSum' and Object_ID = Object_ID(N'MP_ExperianDataCache'))    
BEGIN
	ALTER TABLE MP_ExperianDataCache DROP COLUMN CurrentBalanceSum
END 
GO

