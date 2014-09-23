IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'PayPalFirstTimeWait')
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES ('PayPalFirstTimeWait', '0', 'Integer. Paypal First time update time to wait in milliseconds.', 0)
GO
