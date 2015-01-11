IF OBJECT_ID('PlayLottery') IS NULL
	EXECUTE('CREATE PROCEDURE PlayLottery AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

-------------------------------------------------------------------------------
--
-- Game logic:
--
-- 1. If customer has already played or lottery is closed - do nothing.
--
-- 2. Find number of all the customers that can win (@PlayerCount)
--    in this lottery.
--
-- 3. Find number of available prizes (@PrizeCount) in this lottery.
--
-- 4. If @PrizeCount = 0 customer has lost, game ends.
--
-- 5. Create a list of all the available prizes.
--
-- 6. If @PlayerCount > @PrizeCount append to the list of prizes
--    @PlayerCount - @PrizeCount entries of "anti-prize" (i.e. no win).
--
-- 7. Attach to every item in the list a random number.
--
-- 8. Sort the list by the attached random number.
--
-- 9. Take the first item of the sorted list - this is customer's prize
--    which can be a real prize (won) or an "anti-prize" (lost).
--
-------------------------------------------------------------------------------

ALTER PROCEDURE PlayLottery
@LotteryPlayerID UNIQUEIDENTIFIER,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @StatusReserved INT = 5

	------------------------------------------------------------------------------

	DECLARE @UserID INT
	DECLARE @LotteryID BIGINT

	DECLARE @PlayerID BIGINT
	DECLARE @UniqueID UNIQUEIDENTIFIER
	DECLARE @StatusID BIGINT
	DECLARE @LotteryName NVARCHAR(255)
	DECLARE @CanWin BIT
	DECLARE @HasPlayed BIT
	DECLARE @PlayTime DATETIME

	DECLARE @PlayedNow BIT = 0
	DECLARE @Amount DECIMAL(18, 2) = 0
	DECLARE @PrizeID BIGINT = NULL

	DECLARE @PlayerCount INT = 0
	DECLARE @PrizeCount INT = 0

	------------------------------------------------------------------------------

	SET TRANSACTION ISOLATION LEVEL SERIALIZABLE

	BEGIN TRANSACTION

	------------------------------------------------------------------------------

	SELECT
		@UserID = lp.UserID,
		@UniqueID = lp.UniqueID,
		@LotteryID = lp.LotteryID,
		@PlayerID = lp.PlayerID,
		@StatusID = lp.StatusID,
		@LotteryName = l.LotteryName,
		@CanWin = s.CanWin,
		@HasPlayed = s.HasPlayed,
		@PlayTime = lp.PlayTime,
		@PrizeID = lp.PrizeID,
		@Amount = p.Amount
	FROM
		LotteryPlayers lp
		INNER JOIN Lotteries l
			ON lp.LotteryID = l.LotteryID
			AND @Now BETWEEN l.StartDate AND l.EndDate
			AND l.IsActive = 1
		INNER JOIN LotteryPlayerStatuses s ON lp.StatusID = s.StatusID
		LEFT JOIN LotteryPrizes p ON lp.PrizeID = p.PrizeID
	WHERE
		lp.UniqueID = @LotteryPlayerID

	------------------------------------------------------------------------------

	SET @StatusID = ISNULL(@StatusID, 0)

	------------------------------------------------------------------------------

	IF @Amount IS NULL OR @Amount < 0
		SET @Amount = 0

	------------------------------------------------------------------------------

	IF @PlayerID IS NOT NULL AND @CanWin = 1 AND @HasPlayed = 0
		SET @PlayedNow = 1

	------------------------------------------------------------------------------

	IF @PlayedNow = 1
	BEGIN
		-------------------------------------------------------------------------

		SELECT
			@PlayerCount = COUNT(*)
		FROM
			LotteryPlayers lp
			INNER JOIN LotteryPlayerStatuses s
				ON lp.StatusID = s.StatusID
				AND s.CanWin = 1
		WHERE
			lp.LotteryID = @LotteryID

		-------------------------------------------------------------------------

		CREATE TABLE #prizes (
			PrizeID BIGINT NULL,
			Amount BIGINT NOT NULL,
			Rnd INT NOT NULL
		)

		-------------------------------------------------------------------------

		INSERT INTO #prizes (PrizeID, Amount, Rnd)
		SELECT
			p.PrizeID,
			p.Amount,
			ABS(CHECKSUM(NewId()))
		FROM
			LotteryPrizes p
			LEFT JOIN LotteryPlayers lp
				ON p.PrizeID = lp.PrizeID
				AND p.LotteryID = @LotteryID
		WHERE
			lp.PrizeID IS NULL

		-------------------------------------------------------------------------

		SELECT
			@PrizeCount = COUNT(*)
		FROM
			#prizes

		-------------------------------------------------------------------------

		SET @PrizeCount = ISNULL(@PrizeCount, 0)

		-------------------------------------------------------------------------

		IF @PrizeCount > 0
		BEGIN
			IF @PrizeCount < @PlayerCount
			BEGIN
				DECLARE @i INT = 1

				WHILE @i <= @PlayerCount - @PrizeCount
				BEGIN
					INSERT INTO #prizes (PrizeID, Amount, Rnd)
						VALUES (NULL, 0, ABS(CHECKSUM(NewId())))

					SET @i = @i + 1
				END

				SELECT TOP 1
					@PrizeID = p.PrizeID,
					@Amount = p.Amount
				FROM
					#prizes p
				ORDER BY
					p.Rnd
			END
		END

		-------------------------------------------------------------------------

		SET @StatusID = @StatusReserved
		SET @PlayTime = @Now

		UPDATE LotteryPlayers SET
			StatusID = @StatusID,
			PrizeID = @PrizeID,
			PlayTime = @PlayTime
		WHERE
			PlayerID = @PlayerID

		-------------------------------------------------------------------------

		DROP TABLE #prizes
	END

	------------------------------------------------------------------------------

	COMMIT TRANSACTION

	------------------------------------------------------------------------------

	SELECT
		PlayedNow = @PlayedNow,
		PlayerID = @PlayerID,
		UniqueID = @UniqueID,
		StatusID = @StatusID,
		Status = s.Status,
		CanWin = s.CanWin,
		HasPlayed = s.HasPlayed,
		PlayTime = @PlayTime,
		PrizeID = @PrizeID,
		Amount = @Amount
	FROM
		LotteryPlayerStatuses s
	WHERE
		s.StatusID = @StatusID

	------------------------------------------------------------------------------
END
GO