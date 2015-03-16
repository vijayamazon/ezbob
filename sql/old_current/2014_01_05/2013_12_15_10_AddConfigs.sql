IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='DefaultFeedbackValue')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('DefaultFeedbackValue', '20000', 'Will be used as default feedback in case feedback data is not available')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='TotalTimeToWaitForMarketplacesUpdate')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('TotalTimeToWaitForMarketplacesUpdate', '43200', 'Total time to wait for marketplaces update in seconds')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='IntervalWaitForMarketplacesUpdate')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('IntervalWaitForMarketplacesUpdate', '300000', 'Interval to wait for marketplaces update in milliseconds')
END

GO


