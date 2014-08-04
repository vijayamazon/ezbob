IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'IsOwnerOfMainAddress' and Object_ID = Object_ID(N'CustomerPropertyStatuses'))    
BEGIN
	DECLARE @Statement NVARCHAR(MAX)
	
	SET @Statement = 'ALTER TABLE CustomerPropertyStatuses ADD IsOwnerOfMainAddress BIT'
	
	EXEC(@Statement)
	
	SET @Statement = 'UPDATE CustomerPropertyStatuses SET IsOwnerOfMainAddress = 0'
	
	EXEC(@Statement)
	
	SET @Statement = 'UPDATE 
		CustomerPropertyStatuses 
	SET 
		IsOwnerOfMainAddress = 1
	WHERE 
		Description = ''I own only this property'' OR
		Description = ''I own this property and other properties'' OR
		Description = ''Home owner'''
	
	EXEC(@Statement)
		
	SET @Statement = 'ALTER TABLE CustomerPropertyStatuses ADD IsOwnerOfOtherProperties BIT'
	
	EXEC(@Statement)
	
	SET @Statement = 'UPDATE CustomerPropertyStatuses SET IsOwnerOfOtherProperties = 0'
	
	EXEC(@Statement)
	
	SET @Statement = 'UPDATE 
		CustomerPropertyStatuses 
	SET 
		IsOwnerOfOtherProperties = 1
	WHERE 
		Description = ''I live in the above and own other properties'' OR
		Description = ''I own this property and other properties'''
	
	EXEC(@Statement)
END
GO

