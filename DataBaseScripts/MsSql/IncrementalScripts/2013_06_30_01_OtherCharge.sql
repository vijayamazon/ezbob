IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name='OtherCharge')
BEGIN
	INSERT INTO ConfigurationVariables VALUES ('OtherCharge','10','A custom fee applied by Underwriter')
END
GO
