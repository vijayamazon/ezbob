SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('GetCustomerSecurityQuestion') IS NULL
	EXECUTE('CREATE PROCEDURE GetCustomerSecurityQuestion AS SELECT 1')
GO

ALTER PROCEDURE GetCustomerSecurityQuestion
@Email NVARCHAR(250),
@OriginID INT,
@SecurityQuestion NVARCHAR(200) OUTPUT
AS
BEGIN
	SET @SecurityQuestion = NULL

	SELECT
		@SecurityQuestion = q.Name
	FROM
		Security_User u
		INNER JOIN Security_Question q ON u.SecurityQuestion1Id = q.id
	WHERE
		u.EMail = @Email
		AND
		u.OriginID = @OriginID

	DECLARE @rc INT = @@ROWCOUNT

	RETURN CASE @rc WHEN 1 THEN 0 ELSE 1 END
END
GO
