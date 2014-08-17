IF OBJECT_ID('CustomerPropertyStatuses') IS NULL
BEGIN
	CREATE TABLE CustomerPropertyStatuses 
	(
		Id INT IDENTITY NOT NULL,
		Description NVARCHAR(50),
		IsOwner BIT,
		GroupId INT,
		IsActive BIT
	)
END
GO

IF OBJECT_ID('CustomerPropertyStatusGroups') IS NULL
BEGIN
	CREATE TABLE CustomerPropertyStatusGroups 
	(
		Id INT IDENTITY NOT NULL,
		Name NVARCHAR(20),
		Title NVARCHAR(50),
		Priority INT
	)
END
GO

IF NOT EXISTS (SELECT 1 FROM CustomerPropertyStatusGroups WHERE Title = 'Property-owners')
BEGIN
	DECLARE @Statement NVARCHAR(MAX)
	
	SET @Statement = '
	INSERT INTO CustomerPropertyStatusGroups (Name, Title, Priority) VALUES (''Owners'', ''Property-owners'', 1)
	INSERT INTO CustomerPropertyStatusGroups (Name, Title, Priority) VALUES (''Not owners'', ''I do not own any property'', 2)
	
	INSERT INTO CustomerPropertyStatuses (Description, GroupId, IsActive, IsOwnerOfMainAddress, IsOwnerOfOtherProperties) VALUES (''I own only this property'', 1, 1, 1, 0)
	INSERT INTO CustomerPropertyStatuses (Description, GroupId, IsActive, IsOwnerOfMainAddress, IsOwnerOfOtherProperties) VALUES (''I own this property and other properties'', 1, 1, 1, 1)
	INSERT INTO CustomerPropertyStatuses (Description, GroupId, IsActive, IsOwnerOfMainAddress, IsOwnerOfOtherProperties) VALUES (''I live in the above and own other properties'', 1, 1, 0, 1)
	INSERT INTO CustomerPropertyStatuses (Description, GroupId, IsActive, IsOwnerOfMainAddress, IsOwnerOfOtherProperties) VALUES (''Home owner'', 1, 0, 1, 0)
	
	INSERT INTO CustomerPropertyStatuses (Description, GroupId, IsActive, IsOwnerOfMainAddress, IsOwnerOfOtherProperties) VALUES (''Renting'', 2, 1, 0, 0)
	INSERT INTO CustomerPropertyStatuses (Description, GroupId, IsActive, IsOwnerOfMainAddress, IsOwnerOfOtherProperties) VALUES (''Social house'', 2, 1, 0, 0)
	INSERT INTO CustomerPropertyStatuses (Description, GroupId, IsActive, IsOwnerOfMainAddress, IsOwnerOfOtherProperties) VALUES (''Living with parents'', 2, 1, 0, 0)
	'
	
	EXEC(@Statement)
	
END
GO
