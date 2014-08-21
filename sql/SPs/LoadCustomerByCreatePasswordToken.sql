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
		@CustomerID = ISNULL(c.Id, b.BrokerID),
		@Email = u.Email,
		@FirstName = CASE WHEN c.Id IS NULL THEN b.ContactName ELSE c.FirstName END,
		@LastName = CASE WHEN c.Id IS NULL THEN '' ELSE c.Surname END,
		@DateAccessed = t.DateAccessed
	FROM
		CreatePasswordTokens t
		INNER JOIN Security_User u ON t.CustomerID = u.UserId
		LEFT JOIN Customer c ON u.UserId = c.Id
		LEFT JOIN Broker b ON u.UserId = b.BrokerID
	WHERE
		t.TokenID = @TokenID
		AND (
			t.DateAccessed IS NULL
			OR
			DATEDIFF(minute, t.DateAccessed, @Now) BETWEEN 0 AND 5
		)
		AND
		t.DateDeleted IS NULL

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

	------------------------------------------------------------------------------
END
GO
