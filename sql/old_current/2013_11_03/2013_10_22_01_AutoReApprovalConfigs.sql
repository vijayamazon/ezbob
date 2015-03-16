IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AutoReApproveMaxNumOfOutstandingLoans')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AutoReApproveMaxNumOfOutstandingLoans', 1, 'Maximal number of outstanding loans for a customer to be auto re-approved')
END

GO
