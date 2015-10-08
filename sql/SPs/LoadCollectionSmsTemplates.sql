IF object_id('LoadCollectionSmsTemplates') IS NULL
BEGIN	
	EXECUTE('CREATE PROCEDURE LoadCollectionSmsTemplates AS SELECT 1')
END
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadCollectionSmsTemplates
AS
BEGIN
	SELECT CollectionSmsTemplateID, Type, IsActive, OriginID, Template
    FROM CollectionSmsTemplate
END    
GO