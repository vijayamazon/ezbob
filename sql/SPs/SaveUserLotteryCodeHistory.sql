IF OBJECT_ID('SaveUserLotteryCodeHistory') IS NULL
	EXECUTE('CREATE PROCEDURE SaveUserLotteryCodeHistory AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE SaveUserLotteryCodeHistory
@UserID INT,
@LotteryCode NVARCHAR(64),
@PageVisitTime DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @CodeID BIGINT = NULL

	IF @PageVisitTime IS NOT NULL
	BEGIN
		SELECT
			@CodeID = CodeID
		FROM
			LotteryCodes
		WHERE
			Code = @LotteryCode
	END

	IF @CodeID IS NOT NULL
	BEGIN
		INSERT INTO UserLotteryCodeHistory(UserID, CodeID, PageVisitTime)
			VALUES (@UserID, @CodeID, @PageVisitTime)
	END
END
GO
