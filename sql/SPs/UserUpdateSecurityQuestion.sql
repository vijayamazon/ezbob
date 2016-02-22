IF OBJECT_ID('UserUpdateSecurityQuestion') IS NULL
	EXECUTE('CREATE PROCEDURE UserUpdateSecurityQuestion AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UserUpdateSecurityQuestion
@UserID INT,
@QuestionID BIGINT,
@Answer VARCHAR(200),
@EzPassword NVARCHAR(255),
@Salt NVARCHAR(255),
@CycleCount NVARCHAR(255)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ErrMsg NVARCHAR(255)
	DECLARE @AffectedRows INT = 0

	BEGIN TRAN

	UPDATE Security_User SET
		SecurityQuestion1Id = @QuestionID,
		SecurityAnswer1 = @Answer,
		EzPassword = ISNULL(@EzPassword, EzPassword),
		Salt = ISNULL(@Salt, Salt),
		CycleCount = ISNULL(@CycleCount, CycleCount)
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
