IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'ConnectionPoolMaxSize')
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES ('ConnectionPoolMaxSize', '100', 'Integer. Maximum size for connection pool.', 0)
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'ConnectionPoolReuseCount')
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES ('ConnectionPoolReuseCount', '10', 'Integer. How many times one connection should be reused.', 0)
GO
