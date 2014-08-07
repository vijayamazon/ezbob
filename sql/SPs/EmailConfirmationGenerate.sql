IF OBJECT_ID('EmailConfirmationGenerate') IS NULL
	EXECUTE('CREATE PROCEDURE EmailConfirmationGenerate AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE EmailConfirmationGenerate
@Token UNIQUEIDENTIFIER,
@UserID INT,
@EmailStateID INT,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO EmailConfirmationRequest(Id, CustomerId, [Date], State)
		VALUES (@Token, @UserID, @Now, @EmailStateID)
END
GO
