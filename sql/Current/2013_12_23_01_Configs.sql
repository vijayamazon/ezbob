IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='UseNewMailStrategies')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('UseNewMailStrategies', 'False', 'Determines if we use the new mailing strategies (or scortos older implementations)')
END
ELSE
BEGIN
	UPDATE ConfigurationVariables SET Description = 'Determines if we use the new mailing strategies (or scortos older implementations)' WHERE Name = 'UseNewMailStrategies'
END

GO

