SET QUOTED_IDENTIFIER ON
GO

-- Just a temp variable. Not using real temp variable because variables disappear after "GO".
CREATE TABLE #new_cols (a INT)
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('CustomerOrigin') AND name = 'SearchPriority')
BEGIN
	ALTER TABLE CustomerOrigin ADD SearchPriority INT NULL

	EXECUTE('UPDATE CustomerOrigin SET SearchPriority = 0')

	ALTER TABLE CustomerOrigin ALTER COLUMN SearchPriority INT NOT NULL

	INSERT INTO #new_cols (a) VALUES (1)
END
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('CustomerOrigin') AND name = 'UrlNeedle')
BEGIN
	ALTER TABLE CustomerOrigin ADD UrlNeedle NVARCHAR(255) NULL

	INSERT INTO #new_cols (a) VALUES (1)
END
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('CustomerOrigin') AND name = 'PhoneNumber')
	ALTER TABLE CustomerOrigin ADD PhoneNumber NVARCHAR(32) NULL

	EXECUTE('UPDATE CustomerOrigin SET PhoneNumber = ''0''')

	ALTER TABLE CustomerOrigin ALTER COLUMN PhoneNumber NVARCHAR(32) NOT NULL

	INSERT INTO #new_cols (a) VALUES (1)
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('CustomerOrigin') AND name = 'CustomerCareEmail')
	ALTER TABLE CustomerOrigin ADD CustomerCareEmail NVARCHAR(255) NULL

	EXECUTE('UPDATE CustomerOrigin SET CustomerCareEmail = ''customercare''')

	ALTER TABLE CustomerOrigin ALTER COLUMN CustomerCareEmail NVARCHAR(255) NOT NULL

	INSERT INTO #new_cols (a) VALUES (1)
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('CustomerOrigin') AND name = 'MetaDescription')
BEGIN
	ALTER TABLE CustomerOrigin ADD MetaDescription NVARCHAR(MAX) NULL

	EXECUTE('UPDATE CustomerOrigin SET MetaDescription = ''0''')

	ALTER TABLE CustomerOrigin ALTER COLUMN MetaDescription NVARCHAR(MAX) NOT NULL

	INSERT INTO #new_cols (a) VALUES (1)
END
GO

IF EXISTS (SELECT * FROM #new_cols)
BEGIN
	UPDATE CustomerOrigin SET
		SearchPriority = 10,
		UrlNeedle = 'everline.com',
		PhoneNumber = '0203-371-0322',
		CustomerCareEmail = 'customercare@everline.com',
		MetaDescription = 'Everline provides a fast, flexible and convenient source of credit to entrepreneurs and looking to expand their business or manage working capital.'
	WHERE
		Name = 'everline'

	UPDATE CustomerOrigin SET
		SearchPriority = 0,
		UrlNeedle = 'ezbob.com',
		PhoneNumber = '0800-011-4787',
		CustomerCareEmail = 'customercare@ezbob.com',
		MetaDescription = 'Online sellers sign up for a free account to apply for an ecommerce business loan. Funds can be in your account within 30 minutes.'
	WHERE
		Name = 'ezbob'
END
GO

DROP TABLE #new_cols
GO

IF NOT EXISTS (SELECT * FROM CustomerOrigin WHERE Name = 'alibaba')
BEGIN
	INSERT INTO CustomerOrigin (Name, CustomerSite, SearchPriority, UrlNeedle, PhoneNumber, CustomerCareEmail, MetaDescription) VALUES (
		'alibaba',
		'https://app.alibaba.ezbob.com',
		20,
		'alibaba.ezbob',
		'0800-011-4787',
		'customercare@ezbob.com',
		'Online sellers sign up for a free account to apply for an ecommerce business loan. Funds can be in your account within 30 minutes.'
	)
END
GO
