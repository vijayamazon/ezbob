IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('LotteryPlayerStatuses') AND name = 'HasClaimed')
BEGIN
	ALTER TABLE LotteryPlayerStatuses DROP COLUMN TimestampCounter

	ALTER TABLE LotteryPlayerStatuses ADD HasClaimed BIT NULL

	EXECUTE('UPDATE LotteryPlayerStatuses SET HasClaimed = 1')
	EXECUTE('UPDATE LotteryPlayerStatuses SET HasClaimed = 0 WHERE StatusID = 5')

	ALTER TABLE LotteryPlayerStatuses ALTER COLUMN HasClaimed BIT NOT NULL

	ALTER TABLE LotteryPlayerStatuses ADD TimestampCounter ROWVERSION
END
GO
