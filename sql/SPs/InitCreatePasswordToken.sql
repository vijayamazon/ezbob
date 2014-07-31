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
			c.Id,
			@Now,
			NULL,
			NULL
		FROM
			Customer c
		WHERE
			c.Name = @Email

		IF @@ROWCOUNT = 1
			SELECT CONVERT(BIT, 1) AS Success
		ELSE
			SELECT CONVERT(BIT, 0) AS Success
	END TRY
	BEGIN CATCH
		SELECT CONVERT(BIT, 0) AS Success
	END CATCH
END
GO
