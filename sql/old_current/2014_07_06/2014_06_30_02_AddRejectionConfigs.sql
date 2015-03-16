IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'Reject_Defaults_CompanyScore')
	INSERT INTO ConfigurationVariables(Name, Value, Description) 
	VALUES ('Reject_Defaults_CompanyScore', '20', 'Used as threshold in rejection logic, the min score for company with certain defaults')
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'Reject_Defaults_CompanyAccountsNum')
	INSERT INTO ConfigurationVariables(Name, Value, Description) 
	VALUES ('Reject_Defaults_CompanyAccountsNum', '1', 'Used as threshold in rejection logic, the min num of company default accounts to reject because')
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'Reject_Defaults_CompanyMonthsNum')
	INSERT INTO ConfigurationVariables(Name, Value, Description) 
	VALUES ('Reject_Defaults_CompanyMonthsNum', '24', 'Used as threshold in rejection logic, the num of months for which we look for company default account')
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'Reject_Defaults_CompanyAmount')
	INSERT INTO ConfigurationVariables(Name, Value, Description) 
	VALUES ('Reject_Defaults_CompanyAmount', '1000', 'Used as threshold in rejection logic, the min amount of balance for a default account to be counted for rejection')
GO
