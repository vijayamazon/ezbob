
IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name='NumofAllowedActiveLoans')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description)
	VALUES ('NumofAllowedActiveLoans', 2, 'value used for create opportunity on sales force side logic')
END
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name='MinLoanLifetimeMonths')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description)
	VALUES ('MinLoanLifetimeMonths', 5, 'value used for create opportunity on sales force side logic')
END
GO