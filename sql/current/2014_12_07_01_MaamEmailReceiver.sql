IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'MaamEmailReceiver')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES (
		'MaamEmailReceiver', '', 'Who receives YesMaam email with results.', 0
	)
END
GO
