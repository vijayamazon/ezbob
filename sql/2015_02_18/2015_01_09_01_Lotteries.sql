SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
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
	(5, 'Reserved', 0, 1),
	(0, 'Unknown', 0, 0)

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
