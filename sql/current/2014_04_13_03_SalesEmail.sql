IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'SalesEmail')
	INSERT INTO ConfigurationVariables (Name, Value, Description) VALUES (
		'SalesEmail', 'sales@ezbob.com', 'Sales group email. Can be comma separated list of emails.')
GO
