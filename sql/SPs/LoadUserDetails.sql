IF OBJECT_ID('LoadUserDetails') IS NULL
	EXECUTE('CREATE PROCEDURE LoadUserDetails AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadUserDetails
@UserID INT
AS
BEGIN
	SELECT 
		UserId,
		UserName,
		Email
	FROM 
		Security_User 
	WHERE 
		UserId = @UserID
END
GO
