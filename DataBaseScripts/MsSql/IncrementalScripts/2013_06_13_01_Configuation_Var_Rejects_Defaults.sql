IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'Reject_Defaults_CreditScore')
	INSERT INTO ConfigurationVariables (Name, Value, Description)
		VALUES ('Reject_Defaults_CreditScore', '800', 'Customer with Credit Score below specified value will be checked for default financial accounts')
GO


IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'Reject_Defaults_AccountsNum')
	INSERT INTO ConfigurationVariables (Name, Value, Description)
		VALUES ('Reject_Defaults_AccountsNum', '1', 'Number of default financial accounts that will lead to rejection')
		


IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'Reject_Defaults_Amount')
	INSERT INTO ConfigurationVariables (Name, Value, Description)
		VALUES ('Reject_Defaults_Amount', '300', 'Defaulted Amount of money value that will lead to rejection')
		
		
IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'Reject_Defaults_MonthsNum')
	INSERT INTO ConfigurationVariables (Name, Value, Description)
		VALUES ('Reject_Defaults_MonthsNum', '24', 'Number of months that is used in rejection calculation')