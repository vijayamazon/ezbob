SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LotteryEnlistingTypes') IS NULL
BEGIN
	CREATE TABLE LotteryEnlistingTypes (
		LotteryEnlistingTypeID INT NOT NULL,
		LotteryEnlistingType NVARCHAR(64) NOT NULL,
		Description NVARCHAR(255) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LotteryEnlistingTypes PRIMARY KEY (LotteryEnlistingTypeID),
		CONSTRAINT UC_LotteryEnlistingTypes UNIQUE (LotteryEnlistingType)
	)

	INSERT INTO LotteryEnlistingTypes (LotteryEnlistingTypeID, LotteryEnlistingType, Description) VALUES
		(1, 'LoanCount', 'Has loans and total number of taken loans is less or equal to LoanCount'),
		(2, 'LoanOrAmount', 'Has at least LoanCount loans or taken amount is at least LoanAmount')
END
GO

ALTER TABLE Lotteries DROP COLUMN TimestampCounter
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('Lotteries') AND name = 'IsForCustomer')
BEGIN
	ALTER TABLE Lotteries ADD IsForCustomer BIT NULL

	EXECUTE('UPDATE Lotteries SET IsForCustomer = CASE WHEN LotteryName LIKE ''customer%'' THEN 1 WHEN LotteryName LIKE ''broker%'' THEN 0 ELSE 1 END')

	ALTER TABLE Lotteries ALTER COLUMN IsForCustomer BIT NOT NULL
END
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('Lotteries') AND name = 'LoanCount')
	ALTER TABLE Lotteries ADD LoanCount INT NULL
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('Lotteries') AND name = 'LoanAmount')
	ALTER TABLE Lotteries ADD LoanAmount DECIMAL(18, 2) NULL
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('Lotteries') AND name = 'LotteryCode')
BEGIN
	ALTER TABLE Lotteries ADD LotteryCode NVARCHAR(64) NULL

	EXECUTE('UPDATE Lotteries SET LotteryCode = ''ny2015''')

	ALTER TABLE Lotteries ALTER COLUMN LotteryCode NVARCHAR(64) NOT NULL
END
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('Lotteries') AND name = 'LotteryEnlistingTypeID')
BEGIN
	ALTER TABLE Lotteries ADD LotteryEnlistingTypeID INT NULL

	EXECUTE('UPDATE Lotteries SET LotteryEnlistingTypeID = 1')

	ALTER TABLE Lotteries ALTER COLUMN LotteryEnlistingTypeID INT NOT NULL

	ALTER TABLE Lotteries ADD CONSTRAINT FK_Lottery_EnlistingType FOREIGN KEY (LotteryEnlistingTypeID) REFERENCES LotteryEnlistingTypes(LotteryEnlistingTypeID)
END
GO

ALTER TABLE Lotteries ADD TimestampCounter ROWVERSION
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('LotteryPrizes') AND name = 'LinkedPrizeID')
BEGIN
	ALTER TABLE LotteryPrizes DROP COLUMN TimestampCounter

	ALTER TABLE LotteryPrizes ADD LinkedPrizeID BIGINT NULL
	ALTER TABLE LotteryPrizes ADD CONSTRAINT FK_LotteryPrizes_LinkedPrise FOREIGN KEY (LinkedPrizeID) REFERENCES LotteryPrizes (PrizeID)

	ALTER TABLE LotteryPrizes ADD TimestampCounter ROWVERSION
END
GO
