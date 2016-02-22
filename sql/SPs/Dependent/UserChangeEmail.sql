IF OBJECT_ID('UserChangeEmail') IS NULL
	EXECUTE('CREATE PROCEDURE UserChangeEmail AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UserChangeEmail
@ChangedByUserID INT,
@UserID INT,
@Email NVARCHAR(250),
@EzPassword VARCHAR(255),
@Salt VARCHAR(255),
@CycleCount VARCHAR(255),
@RequestID UNIQUEIDENTIFIER,
@RequestState NVARCHAR(100),
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ErrMsg NVARCHAR(255) = ''
	DECLARE @AffectedRows INT = 0
	DECLARE @OriginID INT
	DECLARE @OldEmail NVARCHAR(250)

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	BEGIN TRAN

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	IF @ErrMsg = ''
	BEGIN
		SELECT
			@OriginID = OriginID,
			@OldEmail = Name
		FROM
			Customer
		WHERE
			Id = @UserID

		-------------------------------------------------------------------------

		IF @OriginID IS NULL
		BEGIN
			SET @ErrMsg = 'User not found.'
			ROLLBACK TRAN
		END
	END

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	IF @ErrMsg = ''
	BEGIN
		IF EXISTS (
			SELECT
				c.Id
			FROM
				Customer c
			WHERE
				c.Name = @Email
				AND
				c.OriginID = @OriginID
			UNION
			SELECT
				b.BrokerID
			FROM
				Broker b
			WHERE
				b.ContactEmail = @Email
		)
		BEGIN
			SET @ErrMsg = 'Email is already being used.'
			ROLLBACK TRAN
		END
	END

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	IF @ErrMsg = ''
	BEGIN
		UPDATE Security_User SET
			UserName = @Email,
			EMail = @Email,
			FullName = @Email,
			EzPassword = @EzPassword,
			Salt = @Salt,
			CycleCount = @CycleCount,
			PassSetTime = @Now
		WHERE
			UserId = @UserID

		-------------------------------------------------------------------------

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
		BEGIN TRY
			INSERT INTO UserEmailHistory (EventTime, ChangedByUserID, UserID, OldEmail, NewEmail)
				VALUES (@Now, @ChangedByUserID, @UserID, @OldEmail, @Email)
		END TRY
		BEGIN CATCH
			SET @ErrMsg = 'Failed to save email change history.'
			ROLLBACK TRAN
		END CATCH
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
