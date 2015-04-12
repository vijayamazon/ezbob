
IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name='LandRegistryUserName')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description) VALUES ('LandRegistryUserName', 'SDulman3000', 'Land registry user name')
END
GO
IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name='LandRegistryPassword')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description) VALUES ('LandRegistryPassword', 'iOwNcavb8fxu080IEtpg1rmAzb5dsBsw7FhuLsJTxYk=', 'encrypted land registry password')
END
GO
IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name='LandRegistryFilePath')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description) VALUES ('LandRegistryFilePath', 'c:\temp\landregistry\', 'Filepath to store land registry attachments')
END
GO
