IF OBJECT_ID('SetCustomerPasswordByToken') IS NULL
	EXECUTE('CREATE PROCEDURE SetCustomerPasswordByToken AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE SetCustomerPasswordByToken
@Email NVARCHAR(128),
@EzPassword VARCHAR(255),
@TokenID UNIQUEIDENTIFIER,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @CustomerID INT = 0

	------------------------------------------------------------------------------

	SELECT
		@CustomerID = c.Id
	FROM
		Customer c
		INNER JOIN CreatePasswordTokens t
			ON c.Id = t.CustomerID
			AND t.TokenID = @TokenID
			AND t.DateAccessed IS NOT NULL
			AND t.DateDeleted IS NULL
	WHERE
		c.Name = @Email

	------------------------------------------------------------------------------

	IF @CustomerID > 0
	BEGIN
		BEGIN TRANSACTION

		-------------------------------------------------------------------------

		UPDATE CreatePasswordTokens SET
			DateDeleted = @Now
		WHERE
			TokenID = @TokenID

		-------------------------------------------------------------------------

		UPDATE Security_User SET
			EzPassword = @EzPassword
		WHERE
			UserId = @CustomerID

		-------------------------------------------------------------------------

		SELECT @CustomerID AS CustomerID

		-------------------------------------------------------------------------

		COMMIT TRANSACTION

		-------------------------------------------------------------------------

		RETURN
	END

	------------------------------------------------------------------------------

	SELECT CONVERT(INT, 0) AS CustomerID
END
GO
