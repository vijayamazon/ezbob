IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='LimitedMedalDaysOfMpRelevancy')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('LimitedMedalDaysOfMpRelevancy', '180', 'If a relevant marketplace wasn''t updated in last this amount of days - limited medal won''t be calculated')
END
GO
