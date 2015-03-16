IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AspireToMinSetupFee')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AspireToMinSetupFee', 'True', 'Defines if the automatic offer tries to minimize or maximize setup fee')
END
GO
