IF NOT EXISTS(SELECT * FROM ConfigurationVariables WHERE Name='AutomaticTestCustomerMark')
BEGIN 
	INSERT INTO dbo.ConfigurationVariables
		(
		Name
		, Value
		, Description
		)
	VALUES
		(
		'AutomaticTestCustomerMark'
		, '0'
		, 'if enabled each new customer with pattern like in TestCustomer Table will be marked as test on registration'
		)
END 
