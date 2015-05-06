	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='CreditSafeUserName')
	BEGIN
		INSERT INTO dbo.ConfigurationVariables(Name, Value, Description, IsEncrypted)
		VALUES('CreditSafeUserName', 'ORAN01', 'CreditSafe Username', NULL)
	END

	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='CreditSafePassword')
	BEGIN
		INSERT INTO dbo.ConfigurationVariables(Name, Value, Description, IsEncrypted)
		VALUES('CreditSafePassword', 'Jd4xDKpy', 'CreditSafe Password', NULL)
	END
