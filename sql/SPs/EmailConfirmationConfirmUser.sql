IF OBJECT_ID('EmailConfirmationConfirmUser') IS NULL
	EXECUTE('CREATE PROCEDURE EmailConfirmationConfirmUser AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE EmailConfirmationConfirmUser
@UserID INT,
@EmailStateID INT
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE EmailConfirmationRequest SET
		State = s.EmailState
	FROM
		EmailConfirmationRequest r
		INNER JOIN EmailConfirmationStates sc
			ON r.State = sc.EmailState
			AND sc.IsFinal = 0
		INNER JOIN EmailConfirmationStates s
			ON s.EmailStateID = @EmailStateID
	WHERE
		r.CustomerId = @UserID
		

	------------------------------------------------------------------------------

	UPDATE Security_User SET
		EmailStateID = @EmailStateID
	WHERE
		UserId = @UserID
END
GO
