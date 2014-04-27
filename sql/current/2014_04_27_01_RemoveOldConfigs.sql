IF EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name = 'UseNewCaisStrategies')
BEGIN
	DELETE FROM ConfigurationVariables WHERE Name IN ('UseNewCaisStrategies','UseNewFraudCheckerStrategy','UseNewMailStrategies','UseNewMainStrategy','UseNewUpdateCustomerMpsStrategy','UseNewUpdateMpStrategy')
END
GO
