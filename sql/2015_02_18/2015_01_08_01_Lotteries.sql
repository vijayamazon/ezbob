SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('Lotteries') IS NULL
BEGIN
	CREATE TABLE Lotteries (
		LotteryID BIGINT NOT NULL,
		LotteryName NVARCHAR(255) NOT NULL,
		MailTemplateName NVARCHAR(255) NOT NULL,
		StartDate DATETIME NOT NULL,
		EndDate DATETIME NOT NULL,
		IsActive BIT NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_Lotteries PRIMARY KEY (LotteryID),
		CONSTRAINT CHK_Lotteries CHECK (RTRIM(LTRIM(LotteryName)) != '')
	)
END
GO

IF OBJECT_ID('LotteryPrizes') IS NULL
BEGIN
	CREATE TABLE LotteryPrizes (
		PrizeID BIGINT NOT NULL,
		LotteryID BIGINT NOT NULL,
		Amount DECIMAL(18, 2) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LotteryPrizes PRIMARY KEY (PrizeID),
		CONSTRAINT FK_LotteryPrizes_Lottery FOREIGN KEY (LotteryID) REFERENCES Lotteries (LotteryID),
		CONSTRAINT CHK_LotteryPrize CHECK (Amount > 0)
	)
END
GO

IF OBJECT_ID('LotteryPlayerStatuses') IS NULL
BEGIN
	CREATE TABLE LotteryPlayerStatuses (
		StatusID BIGINT NOT NULL,
		Status NVARCHAR(255) NOT NULL,
		CanWin BIT NOT NULL,
		HasPlayed BIT NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LotteryPlayerStatuses PRIMARY KEY (StatusID),
		CONSTRAINT CHK_LotteryPlayerStatuses CHECK (LTRIM(RTRIM(Status)) != ''),
		CONSTRAINT UC_LotteryPlayerStatuses UNIQUE (Status)
	)
END
GO

IF OBJECT_ID('LotteryPlayers') IS NULL
BEGIN
	CREATE TABLE LotteryPlayers (
		PlayerID BIGINT IDENTITY(1, 1) NOT NULL,
		UniqueID UNIQUEIDENTIFIER NOT NULL,
		UserID INT NOT NULL,
		LotteryID BIGINT NOT NULL,
		PrizeID BIGINT NULL,
		StatusID BIGINT NOT NULL,
		InsertionTime DATETIME NOT NULL,
		InvitationSentTime DATETIME NULL,
		PlayTime DATETIME NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LotteryPlayers PRIMARY KEY (PlayerID),
		CONSTRAINT UC_LotteryPlayers_ID UNIQUE (UniqueID),
		CONSTRAINT UC_LotteryPlayers_Participation UNIQUE (UserID, LotteryID),
		CONSTRAINT FK_LotteryPlayers_User FOREIGN KEY (UserID) REFERENCES Security_User (UserId),
		CONSTRAINT FK_LotteryPlayers_Lottery FOREIGN KEY (LotteryID) REFERENCES Lotteries (LotteryID),
		CONSTRAINT FK_LotteryPlayers_Prize FOREIGN KEY (PrizeID) REFERENCES LotteryPrizes (PrizeID),
		CONSTRAINT FK_LotteryPlayers_Status FOREIGN KEY (StatusID) REFERENCES LotteryPlayerStatuses (StatusID)
	)
END
GO

SELECT
	StatusID,
	Status,
	CanWin,
	HasPlayed
INTO
	#s
FROM
	LotteryPlayerStatuses
WHERE
	1 = 0

INSERT INTO #s (StatusID, Status, CanWin, HasPlayed) VALUES
	(1, 'Not invited', 1, 0),
	(2, 'Not played', 1, 0),
	(3, 'Excluded', 0, 0),
	(4, 'Played', 0, 1)

INSERT INTO LotteryPlayerStatuses (StatusID, Status, CanWin, HasPlayed)
SELECT
	#s.StatusID,
	#s.Status,
	#s.CanWin,
	#s.HasPlayed
FROM
	#s
	LEFT JOIN LotteryPlayerStatuses s ON #s.StatusID = s.StatusID
WHERE
	s.StatusID IS NULL

DROP TABLE #s
GO
