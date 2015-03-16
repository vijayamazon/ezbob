IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='CustomerStateRefreshInterval')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('CustomerStateRefreshInterval', 120000, 'Every interval dashboard client will fetch the status of the customer. Interval is defined in milliseconds')
END
GO

