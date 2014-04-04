IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='UpdateConsumerDataPeriodDays')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('UpdateConsumerDataPeriodDays', 90, 'Number of days for which the experian cache is valid for')
END
GO
