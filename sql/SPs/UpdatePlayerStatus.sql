IF OBJECT_ID('UpdatePlayerStatus') IS NULL
	EXECUTE('CREATE PROCEDURE UpdatePlayerStatus AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UpdatePlayerStatus
@PlayerID UNIQUEIDENTIFIER,
@StatusID BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @Excluded BIGINT = 3

	UPDATE LotteryPlayers SET
		StatusID = @StatusID,
		PrizeID = CASE WHEN @StatusID = @Excluded THEN NULL ELSE PrizeID END
	WHERE
		UniqueID = @PlayerID
END
GO
