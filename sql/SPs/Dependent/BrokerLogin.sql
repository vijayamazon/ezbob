IF OBJECT_ID('BrokerLogin') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLogin AS SELECT 1')
GO

ALTER PROCEDURE BrokerLogin
@Email NVARCHAR(255),
@Password NVARCHAR(255),
@LotteryCode NVARCHAR(64),
@PageVisitTime DATETIME,
@UiOriginID INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @BrokerId INT

	------------------------------------------------------------------------------

	SELECT
		@BrokerID = BrokerID
	FROM
		Broker b
		INNER JOIN Security_User u ON b.BrokerID = u.UserId
	WHERE
		b.ContactEmail = @Email
		AND
		u.EzPassword = @Password
		AND
		b.OriginID = @UiOriginID

	------------------------------------------------------------------------------

	IF @BrokerID IS NULL
	BEGIN
		SELECT 'Invalid contact person email or password.' AS ErrorMsg
	END
	ELSE BEGIN
		EXECUTE BrokerLoadOwnProperties @BrokerID = @BrokerId
		EXECUTE SaveUserLotteryCodeHistory @BrokerID, @LotteryCode, @PageVisitTime
	END
END
GO
