IF OBJECT_ID('UserDataForLogin') IS NULL
	EXECUTE('CREATE PROCEDURE UserDataForLogin AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON;
GO

ALTER PROCEDURE UserDataForLogin
@Email NVARCHAR(250),
@OriginID INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		UserId AS UserID,
		UserName AS Email,
		Password,
		EzPassword,
		Salt,
		CycleCount,
		CreationDate,
		IsDeleted,
		DisableDate,
		ForcePassChange,
		PassSetTime,
		PassExpPeriod,
		LoginFailedCount,
		DisablePassChange
	FROM
		Security_User
	WHERE
		UserName = @Email
		AND (
			(@OriginID IS NULL AND OriginID IS NULL)
			OR
			(@OriginID IS NOT NULL AND OriginID = @OriginID)
		)
END
GO
