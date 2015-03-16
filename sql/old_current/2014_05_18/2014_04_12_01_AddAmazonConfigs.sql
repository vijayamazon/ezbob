IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AmazonEnableRetrying')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AmazonEnableRetrying', 'True', 'Amazon enable retrying')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AmazonMinorTimeoutInSeconds')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AmazonMinorTimeoutInSeconds', '60', 'Amazon minor timeout in seconds')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AmazonUseLastTimeOut')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AmazonUseLastTimeOut', 'False', 'Amazon use last timeout')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AmazonIterationSettings1Index')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AmazonIterationSettings1Index', '1', 'Amazon iteration settings 1 index')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AmazonIterationSettings1CountRequestsExpectError')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AmazonIterationSettings1CountRequestsExpectError', '10', 'Amazon iteration settings 1 count requests expect error')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AmazonIterationSettings1TimeOutAfterRetryingExpiredInMinutes')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AmazonIterationSettings1TimeOutAfterRetryingExpiredInMinutes', '30', 'Amazon iteration settings 1 timeout after retrying expired in minutes')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AmazonIterationSettings2Index')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AmazonIterationSettings2Index', '2', 'Amazon iteration settings 2 index')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AmazonIterationSettings2CountRequestsExpectError')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AmazonIterationSettings2CountRequestsExpectError', '5', 'Amazon iteration settings 2 count requests expect error')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AmazonIterationSettings2TimeOutAfterRetryingExpiredInMinutes')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AmazonIterationSettings2TimeOutAfterRetryingExpiredInMinutes', '0', 'Amazon iteration settings 2 timeout after retrying expired in minutes')
END
GO
