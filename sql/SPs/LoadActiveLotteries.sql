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
		lp.UniqueID
	FROM
		LotteryPlayers lp
		INNER JOIN Lotteries l
			ON lp.LotteryID = l.LotteryID
			AND @Now BETWEEN l.StartDate AND l.EndDate
			AND l.IsActive = 1
	WHERE
		lp.UserID = @UserID
END
GO
