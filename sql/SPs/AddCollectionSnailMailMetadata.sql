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
@Now DATETIME,
@CollectionSnailMailTemplateID INT 
AS
BEGIN
	INSERT INTO CollectionSnailMailMetadata (CollectionLogID, Name, ContentType, Path, CreateDate,CollectionSnailMailTemplateID) 
	VALUES 	                                (@CollectionLogID, @Name, @ContentType, @Path, @Now, @CollectionSnailMailTemplateID)
END
GO
