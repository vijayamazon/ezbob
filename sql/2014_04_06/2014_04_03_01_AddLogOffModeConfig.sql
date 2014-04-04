IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='LogOffMode')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('LogOffMode', 0, 'Determines the log off mode. 0:WebProd 1:LogOnOfEnv 2:SignUpOfEnv')
END
GO
