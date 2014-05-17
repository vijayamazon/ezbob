IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name='PayPointCardExpiryMonths')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description) VALUES ('PayPointCardExpiryMonths', 3, 'PayPoint validate that the expiry date of card is not extending the amount of months specified')
END 
