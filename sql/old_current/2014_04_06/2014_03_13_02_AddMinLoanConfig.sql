IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='MinLoanAmount')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('MinLoanAmount', 100, 'Duplicated for env config entry XMinLoan - used in state calculation for dashboard')
END
GO

