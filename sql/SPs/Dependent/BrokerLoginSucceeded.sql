SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('BrokerLoginSucceeded') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLoginSucceeded AS SELECT 1')
GO

ALTER PROCEDURE BrokerLoginSucceeded
@BrokerID NVARCHAR(255),
@EzPassword NVARCHAR(255),
@Salt NVARCHAR(255),
@CycleCount NVARCHAR(255),
@LotteryCode NVARCHAR(64),
@PageVisitTime DATETIME
AS
BEGIN
	IF @EzPassword IS NOT NULL
	BEGIN
		UPDATE Security_User SET
			EzPassword = @EzPassword,
			Salt = @Salt,
			CycleCount = @CycleCount
		WHERE
			UserId = @BrokerID
	END

	EXECUTE BrokerLoadOwnProperties @BrokerID = @BrokerID
	EXECUTE SaveUserLotteryCodeHistory @BrokerID, @LotteryCode, @PageVisitTime
END
GO
