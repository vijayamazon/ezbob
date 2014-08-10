IF OBJECT_ID('UserChangeEmail') IS NULL
	EXECUTE('CREATE PROCEDURE UserChangeEmail AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UserChangeEmail
@UserID INT,
@Email NVARCHAR(250),
@EzPassword VARCHAR(255),
@RequestID UNIQUEIDENTIFIER,
@RequestState NVARCHAR(100),
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ErrMsg NVARCHAR(255) = ''
	DECLARE @AffectedRows INT = 0

	BEGIN TRAN

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	UPDATE Security_User SET
		UserName = @Email,
		EMail = @Email,
		FullName = @Email,
		EzPassword = @EzPassword,
		PassSetTime = @Now
	WHERE
		UserId = @UserID

	------------------------------------------------------------------------------

	SET @AffectedRows = @@ROWCOUNT

	IF @AffectedRows = 0
	BEGIN
		SET @ErrMsg = 'User not found.'
		ROLLBACK TRAN
	END
	ELSE IF @AffectedRows > 1
	BEGIN
		SET @ErrMsg = 'Too many rows updated.'
		ROLLBACK TRAN
	END

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	IF @ErrMsg = ''
	BEGIN
		UPDATE Customer SET
			Name = @Email,
			EmailState = @RequestState
		WHERE
			Id = @UserID

		-------------------------------------------------------------------------

		SET @AffectedRows = @@ROWCOUNT
	
		IF @AffectedRows > 1
		BEGIN
			SET @ErrMsg = 'Too many rows updated.'
			ROLLBACK TRAN
		END
	END

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	IF @ErrMsg = ''
	BEGIN
		INSERT INTO EmailConfirmationRequest(Id, CustomerId, Date, State)
			VALUES (@RequestID, @UserID, @Now, @RequestState)

		EXECUTE InitCreatePasswordTokenByUserID @RequestID, @UserID, @Now

		COMMIT TRAN
	END

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SELECT @ErrMsg AS ErrorMessage
END
GO
