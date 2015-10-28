SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE name='CollectionSnailMailTemplateID' AND id=object_id('CollectionSnailMailMetadata'))
BEGIN
ALTER TABLE CollectionSnailMailMetadata ADD CollectionSnailMailTemplateID INT
END
GO
