IF OBJECT_ID('LoadEmailForPasswordReset') IS NULL
	EXECUTE('CREATE PROCEDURE LoadEmailForPasswordReset AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadEmailForPasswordReset
@TargetID INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		Email
	FROM
		Security_User
	WHERE
		UserId = @TargetID
END
GO
