IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='UseNewMailStrategies')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('UseNewMailStrategies', 'False', 'Detrmines if we use the new mailing strategies (or scortos older implementations)')
END

GO
