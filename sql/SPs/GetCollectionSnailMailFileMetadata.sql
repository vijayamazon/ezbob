SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('GetCollectionSnailMailFileMetadata') IS NULL
	EXECUTE('CREATE PROCEDURE GetCollectionSnailMailFileMetadata AS SELECT 1')
GO


ALTER PROCEDURE GetCollectionSnailMailFileMetadata
@CollectionSnailMailMetadataID INT
AS
BEGIN
	SELECT Name, ContentType, Path
	FROM CollectionSnailMailMetadata 
	WHERE CollectionSnailMailMetadataID=@CollectionSnailMailMetadataID
END
GO