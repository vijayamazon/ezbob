SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LotteryCodes') IS NULL
BEGIN
	CREATE TABLE LotteryCodes (
		CodeID BIGINT IDENTITY(1, 1) NOT NULL,
		Code NVARCHAR(64) NOT NULL,
		Description NVARCHAR(256) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LotteryCodes PRIMARY KEY (CodeID),
		CONSTRAINT UC_LotteryCodes UNIQUE (Code),
		CONSTRAINT CHK_LotteryCodes CHECK(LTRIM(RTRIM(Code)) != '')
	)
END
GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('Lotteries') AND name = 'LotteryCode')
BEGIN
	ALTER TABLE Lotteries DROP COLUMN TimestampCounter

	ALTER TABLE Lotteries ADD CodeID BIGINT NULL
	
	ALTER TABLE Lotteries ADD CONSTRAINT FK_Lotteries_Codes FOREIGN KEY (CodeID) REFERENCES LotteryCodes (CodeID)

	EXECUTE('
	INSERT INTO LotteryCodes (Code)
		SELECT DISTINCT
			LotteryCode
		FROM
			Lotteries
		WHERE
			LotteryCode IS NOT NULL
	')
	
	EXECUTE('
		UPDATE Lotteries SET
			CodeID = lc.CodeID
		FROM
			Lotteries l
			INNER JOIN LotteryCodes lc ON l.LotteryCode = lc.Code
	')

	ALTER TABLE Lotteries DROP COLUMN LotteryCode

	ALTER TABLE Lotteries ALTER COLUMN CodeID BIGINT NOT NULL

	ALTER TABLE Lotteries ADD TimestampCounter ROWVERSION
END
GO

IF OBJECT_ID('UserLotteryCodeHistory') IS NULL
BEGIN
	CREATE TABLE UserLotteryCodeHistory (
		EntryID BIGINT IDENTITY(1, 1) NOT NULL,
		UserID INT NOT NULL,
		CodeID BIGINT NOT NULL,
		PageVisitTime DATETIME NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_UserLotteryCodeHistory PRIMARY KEY (EntryID),
		CONSTRAINT FK_UserLotteryCodeHistory_User FOREIGN KEY (UserID) REFERENCES Security_User (UserId),
		CONSTRAINT FK_UserLotteryCodeHistory_Code FOREIGN KEY (CodeID) REFERENCES LotteryCodes (CodeID)
	)
END
GO
