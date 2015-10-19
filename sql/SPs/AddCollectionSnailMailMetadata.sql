SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('AddCollectionSnailMailMetadata') IS NULL
	EXECUTE('CREATE PROCEDURE AddCollectionSnailMailMetadata AS SELECT 1')
GO


ALTER PROCEDURE AddCollectionSnailMailMetadata
@CollectionLogID INT,
@Name NVARCHAR(255),
@ContentType NVARCHAR(255),
@Path NVARCHAR(500),
@Now DATETIME
AS
BEGIN
	INSERT INTO CollectionSnailMailMetadata (CollectionLogID, Name, ContentType, Path, CreateDate) 
	VALUES 	                                (@CollectionLogID, @Name, @ContentType, @Path, @Now)
END
GO
