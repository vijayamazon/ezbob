IF OBJECT_ID('UserDataForLogin') IS NULL
	EXECUTE('CREATE PROCEDURE UserDataForLogin AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON;
GO

ALTER PROCEDURE UserDataForLogin
@Email NVARCHAR(250)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		UserId AS UserID,
		UserName AS Email,
		Password,
		EzPassword,
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
END
GO
