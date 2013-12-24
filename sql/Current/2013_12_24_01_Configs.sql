IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='UseNewUpdateMpStrategy')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('UseNewUpdateMpStrategy', 'False', 'Determines if we use the new update marketplace strategy (or scortos older implementation)')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='UseNewUpdateCustomerMpsStrategy')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('UseNewUpdateCustomerMpsStrategy', 'False', 'Determines if we use the new update customer marketplaces strategy (or scortos older implementation)')
END

GO
