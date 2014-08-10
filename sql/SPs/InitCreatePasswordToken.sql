IF OBJECT_ID('InitCreatePasswordToken') IS NULL
	EXECUTE('CREATE PROCEDURE InitCreatePasswordToken AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE InitCreatePasswordToken
@TokenID UNIQUEIDENTIFIER,
@Email NVARCHAR(128),
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY
		INSERT INTO CreatePasswordTokens(TokenID, CustomerID, DateCreated, DateAccessed, DateDeleted)
		SELECT
			@TokenID,
			c.UserId,
			@Now,
			NULL,
			NULL
		FROM
			Security_User c
		WHERE
			c.Email = @Email

		IF @@ROWCOUNT = 1
			SELECT CONVERT(BIT, 1) AS Success, '' AS ErrorMsg
		ELSE
			SELECT CONVERT(BIT, 0) AS Success, 'Row count is not equal to 1' AS ErrorMsg
	END TRY
	BEGIN CATCH
		SELECT CONVERT(BIT, 0) AS Success, dbo.udfGetErrorMsg() AS ErrorMsg
	END CATCH
END
GO
