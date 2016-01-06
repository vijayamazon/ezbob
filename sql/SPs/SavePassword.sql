IF OBJECT_ID('SavePassword') IS NULL
	EXECUTE('CREATE PROCEDURE SavePassword AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE SavePassword
@TargetID INT,
@Password NVARCHAR(255),
@Salt NVARCHAR(255),
@CycleCount NVARCHAR(255)
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE Security_User SET
		EzPassword = @Password,
		Salt = @Salt,
		CycleCount = @CycleCount
	WHERE
		UserId = @TargetID
END
GO
