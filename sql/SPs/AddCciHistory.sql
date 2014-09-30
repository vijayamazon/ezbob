IF OBJECT_ID('AddCciHistory') IS NULL
	EXECUTE('CREATE PROCEDURE AddCciHistory AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE AddCciHistory
@CustomerId INT,
@UnderwriterID INT,
@CciMark BIT,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;
	
	INSERT INTO CciHistory (Username, CustomerId, ChangeDate, CciMark)
	SELECT
		UserName,
		@CustomerId,
		@Now,
		@CciMark
	FROM
		Security_User
	WHERE
		UserId = @UnderwriterID
END
GO
