IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AddSageIntervalMinutes')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AddSageIntervalMinutes', '3', 'Add sage interval in minutes (customer can''t add more than one sage in this period)')
END
GO
