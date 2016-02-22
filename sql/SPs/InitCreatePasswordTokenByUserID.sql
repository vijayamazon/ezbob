IF OBJECT_ID('InitCreatePasswordTokenByUserID') IS NULL
	EXECUTE('CREATE PROCEDURE InitCreatePasswordTokenByUserID AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE InitCreatePasswordTokenByUserID
@Token UNIQUEIDENTIFIER,
@UserID INT,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	UPDATE CreatePasswordTokens SET
		DateAccessed = @Now
	WHERE
		CustomerID = @UserID
		AND
		DateAccessed IS NULL

	------------------------------------------------------------------------------

	UPDATE CreatePasswordTokens SET
		DateDeleted = @Now
	WHERE
		CustomerID = @UserID
		AND
		DateDeleted IS NULL

	------------------------------------------------------------------------------

	UPDATE Security_User SET
		IsPasswordRestored = 1
	WHERE
		UserId = @UserID

	------------------------------------------------------------------------------

	INSERT INTO CreatePasswordTokens(TokenID, CustomerID, DateCreated, DateAccessed, DateDeleted)
		VALUES (@Token, @UserID, @Now, NULL, NULL)
END
GO
