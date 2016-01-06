SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadSecurityAnswer') IS NULL
	EXECUTE('CREATE PROCEDURE LoadSecurityAnswer AS SELECT 1')
GO

ALTER PROCEDURE LoadSecurityAnswer
@Email NVARCHAR(250),
@OriginID INT,
@UserID INT OUTPUT,
@Answer NVARCHAR(200) OUTPUT
AS
BEGIN
	SET @UserID = 0
	SET @Answer = NULL

	SELECT
		@UserID = u.UserId,
		@Answer = u.SecurityAnswer1
	FROM
		Security_User u
	WHERE
		u.EMail = @Email
		AND
		u.OriginID = @OriginID

	DECLARE @rc INT = @@ROWCOUNT

	RETURN CASE @rc WHEN 1 THEN 0 ELSE 1 END
END
GO
