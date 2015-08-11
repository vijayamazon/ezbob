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