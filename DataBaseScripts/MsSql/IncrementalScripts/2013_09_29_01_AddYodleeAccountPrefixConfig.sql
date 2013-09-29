IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name = 'YodleeAccountPrefix')
BEGIN
	INSERT INTO ConfigurationVariables VALUES ('YodleeAccountPrefix', 'EZBOB', 'Prefix for the account name in Yodlee')
END
GO
