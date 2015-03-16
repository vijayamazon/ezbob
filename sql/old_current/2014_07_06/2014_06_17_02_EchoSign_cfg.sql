IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'EchoSignApiKey')
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted)
		VALUES ('EchoSignApiKey', 'Olxf+n3GlY61qqC74qZ2QVb1cC2P25T/cznyxL5jKZhf5c63KYYT0LplcXngPWxt', 'EchoSign API key', 1)
GO
