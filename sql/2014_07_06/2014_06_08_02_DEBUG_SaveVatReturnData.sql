IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'DEBUG_SaveVatReturnData')
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('DEBUG_SaveVatReturnData', '0', 'Boolean. Turn debugging of SaveVatReturnData on and off.')
GO
