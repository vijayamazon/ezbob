IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='FirstOfMonthStatusMailEnabled')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('FirstOfMonthStatusMailEnabled', 0, 'Determines if a status mail will be sent on the beginning of the month')
END

GO
