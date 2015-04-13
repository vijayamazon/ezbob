IF OBJECT_ID('UpdatePlayerStatus') IS NULL
	EXECUTE('CREATE PROCEDURE UpdatePlayerStatus AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UpdatePlayerStatus
@PlayerID UNIQUEIDENTIFIER,
@StatusID BIGINT,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @NotPlayed BIGINT = 2
	DECLARE @Excluded BIGINT = 3

	UPDATE LotteryPlayers SET
		StatusID = @StatusID,
		PrizeID = CASE WHEN @StatusID = @Excluded THEN NULL ELSE PrizeID END,
		InvitationSentTime = CASE WHEN @StatusID = @NotPlayed THEN @Now ELSE InvitationSentTime END
	WHERE
		UniqueID = @PlayerID

	SELECT
		p.UserID,
		l.WinMailTemplateName,
		pr.Amount,
		pr.Description
	FROM
		LotteryPlayers p
		INNER JOIN LotteryPrizes pr ON p.PrizeID = pr.PrizeID
		INNER JOIN Lotteries l ON p.LotteryID = l.LotteryID
	WHERE
		p.UniqueID = @PlayerID
END
GO
