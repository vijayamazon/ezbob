UPDATE CollectionSnailMailTemplate SET IsActive=0 WHERE IsActive=1 AND Type='Annual77ANotification'

--Update to the correct path of the templates on your machine (replace c:\ezbob\Integration\IMailLib\CollectionTemplates\ with the correct path, the sql user must be bulkadmin type in order to work)

IF (0 = (SELECT COUNT(*) FROM CollectionSnailMailTemplate WHERE IsActive=1 AND Type='Annual77ANotification'))
BEGIN 
	INSERT INTO CollectionSnailMailTemplate (Type, IsLimited, IsActive, OriginID, TemplateName, FileName, Template)
	SELECT  'Annual77ANotification', 0, 1, 1, 'Annual77ANotificationTemplateName', 'annual-77a.docx', BulkColumn
	FROM OPENROWSET(BULK N'c:\ezbob\Integration\IMailLib\CollectionTemplates\annual-77a.docx', SINGLE_BLOB) blob
	
	INSERT INTO CollectionSnailMailTemplate (Type, IsLimited, IsActive, OriginID, TemplateName, FileName, Template)
	SELECT  'Annual77ANotification', 0, 1, 2, 'Annual77ANotificationTemplateName', 'EVLannual-77a.docx', BulkColumn
	FROM OPENROWSET(BULK N'c:\ezbob\Integration\IMailLib\CollectionTemplates\EVLannual-77a.docx', SINGLE_BLOB) blob
END 
GO