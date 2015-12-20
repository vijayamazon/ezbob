UPDATE CollectionSnailMailTemplate SET IsActive=0 WHERE IsActive=1

--Update to the correct path of the templates on your machine (replace c:\ezbob\Integration\IMailLib\CollectionTemplates\ with the correct path, the sql user must be bulkadmin type in order to work)

IF (0 = (SELECT COUNT(*) FROM CollectionSnailMailTemplate WHERE IsActive=1))
BEGIN 
	INSERT INTO CollectionSnailMailTemplate (Type, IsLimited, IsActive, OriginID, TemplateName, FileName, Template)
	SELECT  'CollectionDay7', 1, 1, 1, 'DefaulttemplateComm7BusinessTemplateName', 'notice-of-default-to-business.docx', BulkColumn
	FROM OPENROWSET(BULK N'c:\ezbob\Integration\IMailLib\CollectionTemplates\notice-of-default-to-business.docx', SINGLE_BLOB) blob
	
	INSERT INTO CollectionSnailMailTemplate (Type, IsLimited, IsActive, OriginID, TemplateName, FileName, Template)
	SELECT  'CollectionDay7', 1, 1, 2, 'DefaulttemplateComm7BusinessTemplateName', 'EVLnotice-of-default-to-business.docx', BulkColumn
	FROM OPENROWSET(BULK N'c:\ezbob\Integration\IMailLib\CollectionTemplates\EVLnotice-of-default-to-business.docx', SINGLE_BLOB) blob
	
	INSERT INTO CollectionSnailMailTemplate (Type, IsLimited, IsActive, OriginID, TemplateName, FileName, Template)
	SELECT 'CollectionDay7', 1, 1, 1, 'DefaulttemplateComm7PersonalTemplateName', 'notice-to-guarantor.docx', BulkColumn
	FROM OPENROWSET(BULK N'c:\ezbob\Integration\IMailLib\CollectionTemplates\notice-to-guarantor.docx', SINGLE_BLOB) blob
	
	INSERT INTO CollectionSnailMailTemplate (Type, IsLimited, IsActive, OriginID, TemplateName, FileName, Template)
	SELECT 'CollectionDay7', 1, 1, 2, 'DefaulttemplateComm7PersonalTemplateName', 'EVLnotice-to-guarantor.docx', BulkColumn
	FROM OPENROWSET(BULK N'c:\ezbob\Integration\IMailLib\CollectionTemplates\EVLnotice-to-guarantor.docx', SINGLE_BLOB) blob
	
	INSERT INTO CollectionSnailMailTemplate (Type, IsLimited, IsActive, OriginID, TemplateName, FileName, Template)
	SELECT 'CollectionDay15', 0, 1, 1, 'DefaulttemplateConsumer14TemplateName', 'default-notice.docx', BulkColumn
	FROM OPENROWSET(BULK N'c:\ezbob\Integration\IMailLib\CollectionTemplates\default-notice.docx', SINGLE_BLOB) blob
	
	INSERT INTO CollectionSnailMailTemplate (Type, IsLimited, IsActive, OriginID, TemplateName, FileName, Template)
	SELECT 'CollectionDay15', 0, 1, 2, 'DefaulttemplateConsumer14TemplateName', 'EVLdefault-notice.docx', BulkColumn
	FROM OPENROWSET(BULK N'c:\ezbob\Integration\IMailLib\CollectionTemplates\EVLdefault-notice.docx', SINGLE_BLOB) blob
	
	INSERT INTO CollectionSnailMailTemplate (Type, IsLimited, IsActive, OriginID, TemplateName, FileName, Template)
	SELECT 'CollectionDay15', 0, 1, 1, 'DefaulttemplateConsumer14Attachment', 'information-sheet-default.pdf', BulkColumn
	FROM OPENROWSET(BULK N'c:\ezbob\Integration\IMailLib\CollectionTemplates\information-sheet-default.pdf', SINGLE_BLOB) blob
	
	INSERT INTO CollectionSnailMailTemplate (Type, IsLimited, IsActive, OriginID, TemplateName, FileName, Template)
	SELECT 'CollectionDay15', 0, 1, 2, 'DefaulttemplateConsumer14Attachment', 'information-sheet-default.pdf', BulkColumn
	FROM OPENROWSET(BULK N'c:\ezbob\Integration\IMailLib\CollectionTemplates\information-sheet-default.pdf', SINGLE_BLOB) blob
	
	INSERT INTO CollectionSnailMailTemplate (Type, IsLimited, IsActive, OriginID, TemplateName, FileName, Template)
	SELECT 'CollectionDay15', 1, 1, 1, 'DefaultnoticeComm14BorrowerTemplateName', 'default-notice-to-borrowers.docx', BulkColumn
	FROM OPENROWSET(BULK N'c:\ezbob\Integration\IMailLib\CollectionTemplates\default-notice-to-borrowers.docx', SINGLE_BLOB) blob
	
	INSERT INTO CollectionSnailMailTemplate (Type, IsLimited, IsActive, OriginID, TemplateName, FileName, Template)
	SELECT 'CollectionDay15', 1, 1, 2, 'DefaultnoticeComm14BorrowerTemplateName', 'EVLdefault-notice-to-borrowers.docx', BulkColumn
	FROM OPENROWSET(BULK N'c:\ezbob\Integration\IMailLib\CollectionTemplates\EVLdefault-notice-to-borrowers.docx', SINGLE_BLOB) blob
	
	INSERT INTO CollectionSnailMailTemplate (Type, IsLimited, IsActive, OriginID, TemplateName, FileName, Template)
	SELECT 'CollectionDay31', 0, 1, 1, 'DefaulttemplateConsumer31TemplateName', 'sums-of-arrears.docx', BulkColumn
	FROM OPENROWSET(BULK N'c:\ezbob\Integration\IMailLib\CollectionTemplates\sums-of-arrears.docx', SINGLE_BLOB) blob
	
	INSERT INTO CollectionSnailMailTemplate (Type, IsLimited, IsActive, OriginID, TemplateName, FileName, Template)
	SELECT 'CollectionDay31', 0, 1, 2, 'DefaulttemplateConsumer31TemplateName', 'EVLsums-of-arrears.docx', BulkColumn
	FROM OPENROWSET(BULK N'c:\ezbob\Integration\IMailLib\CollectionTemplates\EVLsums-of-arrears.docx', SINGLE_BLOB) blob
	
	INSERT INTO CollectionSnailMailTemplate (Type, IsLimited, IsActive, OriginID, TemplateName, FileName, Template)
	SELECT 'CollectionDay31', 0, 1, 1, 'DefaulttemplateConsumer31Attachment', 'information-sheet-arrears.pdf', BulkColumn
	FROM OPENROWSET(BULK N'c:\ezbob\Integration\IMailLib\CollectionTemplates\information-sheet-arrears.pdf', SINGLE_BLOB) blob
	
	INSERT INTO CollectionSnailMailTemplate (Type, IsLimited, IsActive, OriginID, TemplateName, FileName, Template)
	SELECT 'CollectionDay31', 0, 1, 2, 'DefaulttemplateConsumer31Attachment', 'information-sheet-arrears.pdf', BulkColumn
	FROM OPENROWSET(BULK N'c:\ezbob\Integration\IMailLib\CollectionTemplates\information-sheet-arrears.pdf', SINGLE_BLOB) blob
	
	INSERT INTO CollectionSnailMailTemplate (Type, IsLimited, IsActive, OriginID, TemplateName, FileName, Template)
	SELECT 'CollectionDay7', 1, 1, 1, 'DefaultwarningComm7GuarantorTemplateName', 'warning-letter-to-guarantors.docx', BulkColumn
	FROM OPENROWSET(BULK N'c:\ezbob\Integration\IMailLib\CollectionTemplates\warning-letter-to-guarantors.docx', SINGLE_BLOB) blob
	
	INSERT INTO CollectionSnailMailTemplate (Type, IsLimited, IsActive, OriginID, TemplateName, FileName, Template)
	SELECT 'CollectionDay7', 1, 1, 2, 'DefaultwarningComm7GuarantorTemplateName', 'EVLwarning-letter-to-guarantors.docx', BulkColumn
	FROM OPENROWSET(BULK N'c:\ezbob\Integration\IMailLib\CollectionTemplates\EVLwarning-letter-to-guarantors.docx', SINGLE_BLOB) blob
	
END 
GO