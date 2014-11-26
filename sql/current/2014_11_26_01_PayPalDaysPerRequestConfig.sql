IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPalDaysPerRequest')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPalDaysPerRequest', '30', 'Amount of days to request transactions from paypal transaction search api')
END
GO
