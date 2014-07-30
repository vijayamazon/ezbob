IF OBJECT_ID('LoadCustomerByCreatePasswordToken') IS NULL
	EXECUTE('CREATE PROCEDURE LoadCustomerByCreatePasswordToken AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadCustomerByCreatePasswordToken
@TokenID UNIQUEIDENTIFIER,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @CustomerID INT = 0
	DECLARE @FirstName NVARCHAR(250)
	DECLARE @LastName NVARCHAR(250)
	DECLARE @Email NVARCHAR(128)
	DECLARE @DateAccessed DATETIME

	------------------------------------------------------------------------------

	SELECT
		@CustomerID = c.Id,
		@Email = c.Name,
		@FirstName = c.FirstName,
		@LastName = c.Surname,
		@DateAccessed = t.DateAccessed
	FROM
		Customer c
		INNER JOIN CreatePasswordTokens t
			ON c.Id = t.CustomerID
			AND t.TokenID = @TokenID
			AND (
				t.DateAccessed IS NULL
				OR
				DATEDIFF(minute, t.DateAccessed, @Now) BETWEEN 0 AND 5
			)
			AND t.DateDeleted IS NULL

	------------------------------------------------------------------------------

	IF @CustomerID > 0 AND @DateAccessed IS NULL
	BEGIN
		UPDATE CreatePasswordTokens SET
			DateAccessed = @Now
		WHERE
			TokenID = @TokenID
	END

	------------------------------------------------------------------------------

	SELECT
		@CustomerID AS CustomerID,
		@Email AS Email,
		@FirstName AS FirstName,
		@LastName AS LastName

END
GO
