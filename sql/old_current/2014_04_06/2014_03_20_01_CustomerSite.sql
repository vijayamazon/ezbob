IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'CustomerSite')
	INSERT INTO ConfigurationVariables (Name, Value, Description)
		VALUES ('CustomerSite', 'https://app.ezbob.com', 'Customer site full URL, i.e. including protocol, host, and port. Used in emails.')
GO
