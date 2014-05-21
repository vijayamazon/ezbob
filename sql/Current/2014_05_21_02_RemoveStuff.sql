IF OBJECT_ID('Security_RoleAppRel') IS NOT NULL
BEGIN
	DROP TABLE Security_RoleAppRel
END
GO

IF OBJECT_ID('Security_Application') IS NOT NULL
BEGIN
	DECLARE @DropStatement NVARCHAR(MAX)
	DECLARE cur CURSOR FOR 
		SELECT 
			'ALTER TABLE ' +  OBJECT_SCHEMA_NAME(parent_object_id) +
			'.[' + OBJECT_NAME(parent_object_id) + 
			'] DROP CONSTRAINT ' + name AS DropStatement
		FROM sys.foreign_keys
		WHERE referenced_object_id = object_id('Security_Application')
	OPEN cur
	FETCH NEXT FROM cur INTO @DropStatement
	WHILE @@FETCH_STATUS = 0
	BEGIN
		EXECUTE(@DropStatement)

		FETCH NEXT FROM cur INTO @DropStatement
	END
	CLOSE cur
	DEALLOCATE cur
	DROP TABLE Security_Application
END
GO
































