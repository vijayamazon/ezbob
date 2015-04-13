SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('CustomerOrigin') AND name = 'FrontendSite')
BEGIN
	ALTER TABLE CustomerOrigin ADD FrontendSite NVARCHAR(255) NULL

	EXECUTE('UPDATE CustomerOrigin SET FrontendSite = ''http://www.ezbob.com'' WHERE name = ''ezbob''')
	EXECUTE('UPDATE CustomerOrigin SET FrontendSite = ''http://www.everline.com'' WHERE name = ''everline''')
	EXECUTE('UPDATE CustomerOrigin SET FrontendSite = ''http://www.ezbob.com'' WHERE name = ''alibaba''')
	
	ALTER TABLE CustomerOrigin ALTER COLUMN FrontendSite NVARCHAR(255) NOT NULL
END
GO
