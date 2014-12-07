IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name LIKE 'AutoApproveAllowedCaisStatusesWithLoan')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES (
		'AutoApproveAllowedCaisStatusesWithLoan', '0,1,2,3', 'Comma separated list of CAIS statuses. Customer that took at least one loan can have only these statuses to be auto approved.', 0
	)
END
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name LIKE 'AutoApproveAllowedCaisStatusesWithoutLoan')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES (
		'AutoApproveAllowedCaisStatusesWithoutLoan', '0,1,2', 'Comma separated list of CAIS statuses. Customer that never took a loan can have only these statuses to be auto approved.', 0
	)
END
GO
