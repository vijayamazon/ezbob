IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'LotteryForCustomers')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES (
		'LotteryForCustomers', '0', 'BIGINT. Lottery ID for non-broker customers that have taken a first loan.', 0
	)
END
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'LotteryForBrokers')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES (
		'LotteryForBrokers', '0', 'BIGINT. Lottery ID for brokers of broker customers that have taken a first loan.', 0
	)
END
GO
