IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('Lotteries') AND name = 'IsForNew')
BEGIN
	ALTER TABLE Lotteries DROP COLUMN TimestampCounter

	ALTER TABLE Lotteries ADD IsForNew BIT NULL

	ALTER TABLE Lotteries ADD TimestampCounter ROWVERSION
END
GO
