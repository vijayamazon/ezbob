IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('Lotteries') AND name = 'LotteryPriority')
BEGIN
	ALTER TABLE Lotteries DROP COLUMN TimestampCounter
	ALTER TABLE Lotteries ADD LotteryPriority INT NULL
	ALTER TABLE Lotteries ADD TimestampCounter ROWVERSION
END
GO
