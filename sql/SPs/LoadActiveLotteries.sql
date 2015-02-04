IF OBJECT_ID('LoadActiveLotteries') IS NULL
	EXECUTE('CREATE PROCEDURE LoadActiveLotteries AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadActiveLotteries
@UserID INT,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		lp.UniqueID,
		lc.Code AS LotteryCode
	FROM
		LotteryPlayers lp
		INNER JOIN Lotteries l
			ON lp.LotteryID = l.LotteryID
			AND @Now BETWEEN l.StartDate AND l.EndDate
			AND l.IsActive = 1
		INNER JOIN LotteryPlayerStatuses ls
			ON lp.StatusID = ls.StatusID
			AND (ls.CanWin = 1 OR ls.HasClaimed = 0)
		INNER JOIN LotteryCodes lc
			ON l.CodeID = lc.CodeID
	WHERE
		lp.UserID = @UserID
	ORDER BY
		dbo.udfMaxInt(0, l.LotteryPriority) DESC,
		l.StartDate,
		l.LotteryName
END
GO
