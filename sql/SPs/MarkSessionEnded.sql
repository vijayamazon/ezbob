IF OBJECT_ID('MarkSessionEnded') IS NULL
	EXECUTE('CREATE PROCEDURE MarkSessionEnded AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE MarkSessionEnded
@SessionID INT,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE CustomerSession SET
		EndSession = @Now
	WHERE
		Id = @SessionID
END
GO
