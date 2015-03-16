DECLARE @DefaultConstraint SYSNAME,
	@DropStmt NVARCHAR(500)

SELECT @DefaultConstraint = dc.name
	FROM sys.default_constraints dc
	INNER JOIN syscolumns c
		ON dc.parent_column_id = c.colorder
		AND dc.parent_object_id = c.id
		AND c.name = 'Active'
		AND c.id = OBJECT_ID('MP_MarketplaceType')
		
SET @DropStmt = 'ALTER TABLE MP_MarketplaceType DROP CONSTRAINT ' + @DefaultConstraint

EXEC(@DropStmt)
GO

DECLARE @DefaultConstraint SYSNAME,
	@DropStmt NVARCHAR(500)

SELECT @DefaultConstraint = dc.name
	FROM sys.default_constraints dc
	INNER JOIN syscolumns c
		ON dc.parent_column_id = c.colorder
		AND dc.parent_object_id = c.id
		AND c.name = 'IsOffline'
		AND c.id = OBJECT_ID('MP_MarketplaceType')
		
SET @DropStmt = 'ALTER TABLE MP_MarketplaceType DROP CONSTRAINT ' + @DefaultConstraint

EXEC(@DropStmt)
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'Active' and Object_ID = Object_ID(N'MP_MarketplaceType'))
BEGIN 
	ALTER TABLE MP_MarketplaceType DROP COLUMN Active
END 
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'IsOffline' and Object_ID = Object_ID(N'MP_MarketplaceType'))
BEGIN 
	ALTER TABLE MP_MarketplaceType DROP COLUMN IsOffline
END 
GO

