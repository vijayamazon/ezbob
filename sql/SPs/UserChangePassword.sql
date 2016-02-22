IF OBJECT_ID('UserChangePassword') IS NULL
	EXECUTE('CREATE PROCEDURE UserChangePassword AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UserChangePassword
@UserID INT,
@EzPassword VARCHAR(255),
@Salt VARCHAR(255),
@CycleCount VARCHAR(255),
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ErrMsg NVARCHAR(255)
	DECLARE @AffectedRows INT = 0

	BEGIN TRAN

	UPDATE Security_User SET
		EzPassword = @EzPassword,
		Salt = @Salt,
		CycleCount = @CycleCount,
		PassSetTime = @Now,
		LoginFailedCount = NULL,
		LastBadLogin = NULL,
		ForcePassChange = NULL,
		IsPasswordRestored = 0
	WHERE
		UserId = @UserID

	SET @AffectedRows = @@ROWCOUNT

	IF @AffectedRows = 0
	BEGIN
		ROLLBACK TRAN

		SET @ErrMsg = 'User not found.'
	END
	ELSE	IF @AffectedRows = 1
	BEGIN
		COMMIT TRAN
	END
	ELSE BEGIN -- should never happen
		ROLLBACK TRAN

		SET @ErrMsg = 'Too many rows updated.'
	END

	SELECT @ErrMsg AS ErrorMessage
END
GO
