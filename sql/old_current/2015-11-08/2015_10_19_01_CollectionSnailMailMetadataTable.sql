SET QUOTED_IDENTIFIER ON
GO

IF object_id('CollectionSnailMailMetadata') IS NULL
BEGIN
	CREATE TABLE CollectionSnailMailMetadata (
		CollectionSnailMailMetadataID INT NOT NULL IDENTITY(1,1),
		CollectionLogID INT NOT NULL,
		Name NVARCHAR(255),
		ContentType NVARCHAR(255),
		Path NVARCHAR(500),
		CreateDate DATETIME,
		TimestampCounter ROWVERSION
		CONSTRAINT PK_CollectionSnailMailMetadata PRIMARY KEY (CollectionSnailMailMetadataID),
		CONSTRAINT FK_CollectionSnailMailMetadata_CollectionLog FOREIGN KEY (CollectionLogID) REFERENCES CollectionLog(CollectionLogID),
	)
END
GO
