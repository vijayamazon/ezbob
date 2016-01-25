SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'LogicalGlueEnabled')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES (
		'LogicalGlueEnabled',
		'1',
		'Boolean. Calls to Logical Glue are enabled or not.',
		0
	)
END
GO
