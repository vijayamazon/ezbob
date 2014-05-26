IF OBJECT_ID('UserLoginCheckResult') IS NULL
	EXECUTE('CREATE PROCEDURE UserLoginCheckResult AS SELECT 1')
GO

ALTER PROCEDURE UserLoginCheckResult
@UserID INT,
@EzPassword VARCHAR(255),
@IsDeleted INT,
@LastBadLogin DATETIME,
@LoginFailedCount INT,
@Success BIT,
@ErrorMessage NVARCHAR(50),
@Ip NVARCHAR(50),
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @SessionID INT = 0
	DECLARE @EndSession DATETIME

	UPDATE Security_User SET
		EzPassword = ISNULL(@EzPassword, EzPassword),
		IsDeleted = ISNULL(@IsDeleted, IsDeleted),
		LastBadLogin = @LastBadLogin,
		LoginFailedCount = ISNULL(@LoginFailedCount, LoginFailedCount)
	WHERE
		UserId = @UserID

	SET @EndSession = CASE @Success WHEN 1 THEN NULL ELSE @Now END

	EXECUTE CreateCustomerSession @UserID, @Now, @Ip, @Success, @ErrorMessage, @EndSession, @SessionID OUTPUT
END
GO
