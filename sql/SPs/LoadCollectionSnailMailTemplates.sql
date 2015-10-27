SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadCollectionSnailMailTemplates') IS NULL
	EXECUTE('CREATE PROCEDURE LoadCollectionSnailMailTemplates AS SELECT 1')
GO

ALTER PROCEDURE LoadCollectionSnailMailTemplates
AS
BEGIN
	SELECT
		CollectionSnailMailTemplateID, 
		Type,
		IsLimited,
		IsActive,
		OriginID,
		FileName,
		TemplateName, 
		Template
	FROM 
		CollectionSnailMailTemplate
	WHERE 
		IsActive = 1
END    
GO
