IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='RecentCustomersToKeep')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('RecentCustomersToKeep', 5, 'Determines the number of the recent customers that will be kept')
END


GO
