IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'AutomaticTestBrokerMark')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES (
		'AutomaticTestBrokerMark', '1', 'if enabled each new broker with pattern like in TestCustomer Table will be marked as test on registration'
	)
END
GO
