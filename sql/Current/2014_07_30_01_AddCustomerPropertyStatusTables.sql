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

IF NOT EXISTS (SELECT 1 FROM CustomerPropertyStatusGroups WHERE Name = 'Property-owners')
BEGIN
	INSERT INTO CustomerPropertyStatusGroups (Name, Title, Priority) VALUES ('Owners', 'Property-owners', 1)
	INSERT INTO CustomerPropertyStatusGroups (Name, Title, Priority) VALUES ('Not owners', 'I do not own any property', 2)
	
	INSERT INTO CustomerPropertyStatuses (Description, IsOwner, GroupId, IsActive) VALUES ('I own only this property', 1, 1, 1)
	INSERT INTO CustomerPropertyStatuses (Description, IsOwner, GroupId, IsActive) VALUES ('I own this property and other properties', 1, 1, 1)
	INSERT INTO CustomerPropertyStatuses (Description, IsOwner, GroupId, IsActive) VALUES ('I live in the above and own other properties', 1, 1, 1)
	INSERT INTO CustomerPropertyStatuses (Description, IsOwner, GroupId, IsActive) VALUES ('Home owner', 1, 1, 0)
	
	INSERT INTO CustomerPropertyStatuses (Description, IsOwner, GroupId, IsActive) VALUES ('Renting', 0, 2, 1)
	INSERT INTO CustomerPropertyStatuses (Description, IsOwner, GroupId, IsActive) VALUES ('Social house', 0, 2, 1)
	INSERT INTO CustomerPropertyStatuses (Description, IsOwner, GroupId, IsActive) VALUES ('Living with parents', 0, 2, 1)
END
GO
