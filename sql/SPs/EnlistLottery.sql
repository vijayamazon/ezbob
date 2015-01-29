IF OBJECT_ID('EnlistLottery') IS NULL
	EXECUTE('CREATE PROCEDURE EnlistLottery AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE EnlistLottery
@UserID INT,
@UniqueID UNIQUEIDENTIFIER,
@LotteryID BIGINT,
@IsBroker BIT,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @Email NVARCHAR(255)
	DECLARE @ContactName NVARCHAR(255)

	------------------------------------------------------------------------------

	IF @IsBroker = 0
	BEGIN
		SELECT
			@Email = Name,
			@ContactName = FirstName
		FROM
			Customer c
		WHERE
			c.Id = @UserID
	END
	ELSE BEGIN
		SELECT
			@Email = ContactEmail,
			@ContactName = ContactName
		FROM
			Broker
		WHERE
			BrokerID = @UserID
	END

	------------------------------------------------------------------------------

	IF NOT EXISTS (SELECT * FROM Lotteries WHERE LotteryID = @LotteryID)
		RETURN

	------------------------------------------------------------------------------

	SET TRANSACTION ISOLATION LEVEL SERIALIZABLE

	BEGIN TRANSACTION

	------------------------------------------------------------------------------

	IF EXISTS (SELECT * FROM LotteryPlayers WHERE LotteryID = @LotteryID AND UserID = @UserID)
	BEGIN
		COMMIT TRANSACTION
		RETURN
	END

	INSERT INTO LotteryPlayers (UniqueID, UserID, LotteryID, StatusID, InsertionTime)
		VALUES (@UniqueID, @UserID, @LotteryID, 1, @Now)

	------------------------------------------------------------------------------

	COMMIT TRANSACTION

	------------------------------------------------------------------------------

	SELECT
		Email = @Email,
		ContactName = @ContactName,
		TemplateName = l.MailTemplateName
	FROM
		Lotteries l
	WHERE
		l.LotteryID = @LotteryID
END
GO
