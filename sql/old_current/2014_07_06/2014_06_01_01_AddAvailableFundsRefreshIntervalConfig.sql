IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AvailableFundsRefreshInterval')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AvailableFundsRefreshInterval', '120000', 'Interval for refreshing the available funds in milliseconds')
END
GO
