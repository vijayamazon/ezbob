IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'RejectionExceptionMaxCompanyScore')
	INSERT INTO ConfigurationVariables(Name, Value, Description) 
	VALUES ('RejectionExceptionMaxCompanyScore', '40', 'Used as threshold in rejection exception logic')
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'RejectionExceptionMaxCompanyScoreForMpError')
	INSERT INTO ConfigurationVariables(Name, Value, Description) 
	VALUES ('RejectionExceptionMaxCompanyScoreForMpError', '10', 'Used as threshold in rejection exception logic')
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'RejectionExceptionMaxConsumerScoreForMpError')
	INSERT INTO ConfigurationVariables(Name, Value, Description) 
	VALUES ('RejectionExceptionMaxConsumerScoreForMpError', '500', 'Used as threshold in rejection exception logic')
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'RejectionCompanyScore')
	INSERT INTO ConfigurationVariables(Name, Value, Description) 
	VALUES ('RejectionCompanyScore', '10', 'Used as threshold in rejection logic')
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'RejectByCompanyNumOfDefaultAccounts')
	INSERT INTO ConfigurationVariables(Name, Value, Description) 
	VALUES ('RejectByCompanyNumOfDefaultAccounts', '1', 'Used as threshold in rejection logic')
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'RejectByCompany_Defaults_MonthsNum')
	INSERT INTO ConfigurationVariables(Name, Value, Description) 
	VALUES ('RejectByCompany_Defaults_MonthsNum', '24', 'Used as threshold in rejection logic')
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'RejectByCompany_Defaults_Amount')
	INSERT INTO ConfigurationVariables(Name, Value, Description) 
	VALUES ('RejectByCompany_Defaults_Amount', '1000', 'Used as threshold in rejection logic')
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'RejectByCompanyDefaultsScore')
	INSERT INTO ConfigurationVariables(Name, Value, Description) 
	VALUES ('RejectByCompanyDefaultsScore', '20', 'Used as threshold in rejection logic')
GO

