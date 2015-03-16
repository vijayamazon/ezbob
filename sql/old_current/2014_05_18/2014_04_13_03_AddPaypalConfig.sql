IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPalTransactionSearchMonthsBack')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPalTransactionSearchMonthsBack', '12', 'PayPal transaction search months back')
END
GO
IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPalOpenTimeoutInMinutes')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPalOpenTimeoutInMinutes', '20', 'PayPal open timeout in minutes')
END
GO
IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPalSendTimeoutInMinutes')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPalSendTimeoutInMinutes', '20', 'PayPal send timeout in minutes')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPalEnableRetrying')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPalEnableRetrying', 'True', 'PayPal enable retrying')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPalMinorTimeoutInSeconds')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPalMinorTimeoutInSeconds', '120', 'PayPal minor timeout in seconds')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPalUseLastTimeOut')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPalUseLastTimeOut', 'False', 'PayPal use last timeout')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPalIterationSettings1Index')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPalIterationSettings1Index', '1', 'PayPal iteration settings 1 index')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPalIterationSettings1CountRequestsExpectError')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPalIterationSettings1CountRequestsExpectError', '2', 'PayPal iteration settings 1 count requests expect error')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPalIterationSettings1TimeOutAfterRetryingExpiredInMinutes')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPalIterationSettings1TimeOutAfterRetryingExpiredInMinutes', '720', 'PayPal iteration settings 1 timeout after retrying expired in minutes')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPalIterationSettings2Index')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPalIterationSettings2Index', '2', 'PayPal iteration settings 2 index')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPalIterationSettings2CountRequestsExpectError')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPalIterationSettings2CountRequestsExpectError', '2', 'PayPal iteration settings 2 count requests expect error')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPalIterationSettings2TimeOutAfterRetryingExpiredInMinutes')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPalIterationSettings2TimeOutAfterRetryingExpiredInMinutes', '1080', 'PayPal iteration settings 2 timeout after retrying expired in minutes')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPalIterationSettings3Index')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPalIterationSettings3Index', '3', 'PayPal iteration settings 3 index')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPalIterationSettings3CountRequestsExpectError')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPalIterationSettings3CountRequestsExpectError', '2', 'PayPal iteration settings 3 count requests expect error')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPalIterationSettings3TimeOutAfterRetryingExpiredInMinutes')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPalIterationSettings3TimeOutAfterRetryingExpiredInMinutes', '0', 'PayPal iteration settings 3 timeout after retrying expired in minutes')
END
GO








