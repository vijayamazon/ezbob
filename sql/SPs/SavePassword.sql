IF OBJECT_ID('SavePassword') IS NULL
	EXECUTE('CREATE PROCEDURE SavePassword AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE SavePassword
@TargetID INT,
@Target NVARCHAR(32),
@Password NVARCHAR(255)
AS
BEGIN
	SET NOCOUNT ON;

	IF @Target = 'Customer'
		UPDATE Security_User SET EzPassword = @Password WHERE UserId = @TargetID
	ELSE IF @Target = 'Broker'
	BEGIN
		UPDATE Broker SET
			Password = @Password
		WHERE
			BrokerID = @TargetID

		UPDATE Security_User SET
			EzPassword = @Password
		FROM
			Security_User u
			INNER JOIN Broker b ON u.UserId = b.UserID
		WHERE
			b.BrokerID = @TargetID
	END
END
GO
