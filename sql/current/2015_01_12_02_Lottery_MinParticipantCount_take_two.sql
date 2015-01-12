IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('Lotteries') AND name = 'MinParticipantCount')
BEGIN
	ALTER TABLE Lotteries DROP COLUMN TimestampCounter

	ALTER TABLE Lotteries ADD MinParticipantCount INT NULL

	ALTER TABLE Lotteries ADD TimestampCounter ROWVERSION
END
GO

DELETE FROM ConfigurationVariables WHERE Name = 'LotteryMinParticipantCount'
GO
