IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='OnlineMedalTurnoverCutoff')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('OnlineMedalTurnoverCutoff', '0.7', 'Defines how close the offline turnover data should be to the online turnover data in order to be used by the medal')
END
GO
	