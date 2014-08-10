IF OBJECT_ID('InitCreatePasswordToken') IS NULL
	EXECUTE('CREATE PROCEDURE InitCreatePasswordToken AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE InitCreatePasswordToken
@TokenID UNIQUEIDENTIFIER,
@Email NVARCHAR(128),
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @UserID INT = NULL

	------------------------------------------------------------------------------

	SELECT
		@UserID = UserId
	FROM
		Security_User
	WHERE
		Email = @Email

	------------------------------------------------------------------------------

	IF @UserID IS NULL
		SELECT CONVERT(BIT, 0) AS Success
	ELSE
	BEGIN
		-------------------------------------------------------------------------

		SET TRANSACTION ISOLATION LEVEL SERIALIZABLE

		BEGIN TRANSACTION

		EXECUTE InitCreatePasswordTokenByUserID @TokenID, @UserID, @Now

		COMMIT TRANSACTION

		-------------------------------------------------------------------------

		SELECT CONVERT(BIT, 1) AS Success
	END
END
GO
