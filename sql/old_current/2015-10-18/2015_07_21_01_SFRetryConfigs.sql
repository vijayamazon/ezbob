IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name='SalesForceNumberOfRetries')
BEGIN 
	INSERT INTO dbo.ConfigurationVariables(Name, Value, Description, IsEncrypted)
	VALUES('SalesForceNumberOfRetries', '3', 'SalesForce number of retry attempts ', NULL)
END 	
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name='SalesForceRetryWaitSeconds')
BEGIN 
INSERT INTO dbo.ConfigurationVariables(Name, Value, Description, IsEncrypted)
	VALUES('SalesForceRetryWaitSeconds', '5', 'SalesForce retrying each num of seconds', NULL)
END 	
GO