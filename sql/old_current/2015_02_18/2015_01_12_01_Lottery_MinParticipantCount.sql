IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'LotteryMinParticipantCount')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES (
		'LotteryMinParticipantCount', '140', 'Int. Minimal number of participants in lottery.', 0
	)
END
GO
