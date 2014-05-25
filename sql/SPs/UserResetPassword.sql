IF OBJECT_ID('UserResetPassword') IS NULL
	EXECUTE('CREATE PROCEDURE UserResetPassword AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UserResetPassword
@Email NVARCHAR(250),
@EzPassword VARCHAR(255),
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE Security_User SET
		EzPassword = @EzPassword,
		PassSetTime = @Now,
		LoginFailedCount = NULL,
		LastBadLogin = NULL,
		ForcePassChange = NULL
	WHERE
		UserName = @Email
		AND
		(
			DisablePassChange IS NULL
			OR
			DisablePassChange = 0
		)

	SELECT @@ROWCOUNT AS AffectedRowsCount
END
GO
