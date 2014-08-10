IF OBJECT_ID('LoadEmailConfirmationState') IS NULL
	EXECUTE('CREATE PROCEDURE LoadEmailConfirmationState AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadEmailConfirmationState
@UserID INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		u.UserID,
		u.EmailStateID,
		s.EmailState,
		s.IsFinal,
		s.IsConfirmed
	FROM
		Security_User u
		INNER JOIN EmailConfirmationStates s ON u.EmailStateID = s.EmailStateID
	WHERE
		u.UserId = @UserID
END
GO
