IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='FCFFactor')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('FCFFactor', '3', 'The sum of the current balance will be divided by this factor')
END
GO
