IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'MaxCapHomeOwner')
	INSERT INTO ConfigurationVariables (Name, Value, Description)
		VALUES ('MaxCapHomeOwner', '40000', 'Maximum value that can be granted to a customer if he is a Home owner')
GO


IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'MaxCapNotHomeOwner')
	INSERT INTO ConfigurationVariables (Name, Value, Description)
		VALUES ('MaxCapNotHomeOwner', '20000', 'Maximum value that can be granted to a customer if he is NOT a Home owner')
GO
