IF OBJECT_ID('BrokerUpdatePassword') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerUpdatePassword AS SELECT 1')
GO

ALTER PROCEDURE BrokerUpdatePassword
@ContactEmail NVARCHAR(255),
@OldPassword NVARCHAR(255),
@NewPassword NVARCHAR(255)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @BrokerID INT = 0

	------------------------------------------------------------------------------

	IF @OldPassword != @NewPassword
	BEGIN
		SELECT
			@BrokerID = u.UserID
		FROM
			Security_User u
		WHERE
			u.Email = @ContactEmail
			AND
			u.EzPassword = @OldPassword

		-------------------------------------------------------------------------

		UPDATE Security_User SET
			EzPassword = @NewPassword
		WHERE
			UserId = @BrokerID

		-------------------------------------------------------------------------

		UPDATE Broker SET
			Password = 'not used'
		WHERE
			BrokerID = @BrokerID
	END

	------------------------------------------------------------------------------

	SELECT @BrokerID AS BrokerID
END
GO
