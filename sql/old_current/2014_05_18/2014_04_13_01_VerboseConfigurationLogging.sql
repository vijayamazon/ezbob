IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'VerboseConfigurationLogging')
	INSERT INTO ConfigurationVariables(Name, Value, Description)
		VALUES ('VerboseConfigurationLogging', '1', 'Boolean. Turn writing to log any ConfigurationVariable request on or off.')
GO
