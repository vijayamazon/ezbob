IF OBJECT_ID('EmailConfirmationCheckOne') IS NULL
	EXECUTE('CREATE PROCEDURE EmailConfirmationCheckOne AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE EmailConfirmationCheckOne
@Token UNIQUEIDENTIFIER,
@ExplicitStateID INT,
@ImplicitStateID INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @UserID INT = NULL
	DECLARE @IsFinal BIT

	------------------------------------------------------------------------------

	SELECT TOP 1
		@UserID = r.CustomerId,
		@IsFinal = s.IsFinal
	FROM
		EmailConfirmationRequest r
		INNER JOIN EmailConfirmationStates s ON r.State = s.EmailState
	WHERE
		r.Id = @Token

	------------------------------------------------------------------------------

	IF @UserID IS NULL
	BEGIN
		SELECT CONVERT(INT, 2) AS Response
		RETURN
	END

	------------------------------------------------------------------------------

	IF @IsFinal = 1
	BEGIN
		SELECT CONVERT(INT, 3) AS Response
		RETURN
	END

	------------------------------------------------------------------------------

	UPDATE EmailConfirmationRequest SET
		State = s.EmailState
	FROM
		EmailConfirmationRequest r
		INNER JOIN EmailConfirmationStates s
			ON r.Id = @Token
			AND s.EmailStateID = @ExplicitStateID

	------------------------------------------------------------------------------

	UPDATE EmailConfirmationRequest SET
		State = s.EmailState
	FROM
		EmailConfirmationRequest r
		INNER JOIN EmailConfirmationStates sc
			ON r.State = sc.EmailState
			AND sc.IsFinal = 0
		INNER JOIN EmailConfirmationStates s
			ON r.Id != @Token
			AND r.CustomerId = @UserID
			AND s.EmailStateID = @ImplicitStateID

	------------------------------------------------------------------------------

	UPDATE Security_User SET
		EmailStateID = @ExplicitStateID
	WHERE
		UserId = @UserID

	------------------------------------------------------------------------------

	SELECT CONVERT(INT, 1) AS Response
END
GO
