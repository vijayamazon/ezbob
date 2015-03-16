SET QUOTED_IDENTIFIER ON
GO

DECLARE @Ezbob INT = (SELECT CustomerOriginID FROM CustomerOrigin WHERE Name = 'Ezbob')

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('EsignTemplates') AND Name = 'CustomerOriginID')
BEGIN
	ALTER TABLE EsignTemplates DROP COLUMN TimestampCounter

	ALTER TABLE EsignTemplates ADD CustomerOriginID INT NULL

	EXECUTE('UPDATE EsignTemplates SET CustomerOriginID = ' + @Ezbob)

	ALTER TABLE EsignTemplates ADD CONSTRAINT FK_EsignTemplates_CustomerOrigin FOREIGN KEY(CustomerOriginID) REFERENCES CustomerOrigin (CustomerOriginID)

	ALTER TABLE EsignTemplates ALTER COLUMN CustomerOriginID INT NOT NULL

	ALTER TABLE EsignTemplates ADD TimestampCounter ROWVERSION
END
GO

DECLARE @Now DATETIME = GETUTCDATE()
DECLARE @Everline INT = (SELECT CustomerOriginID FROM CustomerOrigin WHERE Name = 'Everline')

IF NOT EXISTS (SELECT * FROM EsignTemplates WHERE EsignTemplateID = 3)
BEGIN
	INSERT INTO EsignTemplates (EsignTemplateID, DateAdded, EsignTemplateTypeID, FileExtension, Template, CustomerOriginID)
	SELECT
		3, @Now, EsignTemplateTypeID, FileExtension, Template, @Everline
	FROM
		EsignTemplates
	WHERE
		EsignTemplateID = 1
END
GO

DECLARE @Now DATETIME = GETUTCDATE()
DECLARE @Everline INT = (SELECT CustomerOriginID FROM CustomerOrigin WHERE Name = 'Everline')

IF NOT EXISTS (SELECT * FROM EsignTemplates WHERE EsignTemplateID = 4)
BEGIN
	INSERT INTO EsignTemplates (EsignTemplateID, DateAdded, EsignTemplateTypeID, FileExtension, Template, CustomerOriginID)
	SELECT
		4, @Now, EsignTemplateTypeID, FileExtension, Template, @Everline
	FROM
		EsignTemplates
	WHERE
		EsignTemplateID = 2
END
GO
