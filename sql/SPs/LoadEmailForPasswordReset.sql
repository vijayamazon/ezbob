IF OBJECT_ID('LoadEmailForPasswordReset') IS NULL
	EXECUTE('CREATE PROCEDURE LoadEmailForPasswordReset AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadEmailForPasswordReset
@TargetID INT,
@Email NVARCHAR(255) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @Email = NULL

	SELECT
		@Email = Email
	FROM
		Security_User
	WHERE
		UserId = @TargetID
END
GO
