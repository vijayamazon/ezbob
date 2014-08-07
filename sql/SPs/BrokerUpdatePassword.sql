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
	DECLARE @UserID INT = 0

	------------------------------------------------------------------------------

	IF @OldPassword != @NewPassword
	BEGIN
		SELECT
			@BrokerID = b.BrokerID,
			@UserID = u.UserId
		FROM
			Broker b
			INNER JOIN Security_User u ON b.UserID = u.UserId
		WHERE
			b.ContactEmail = @ContactEmail
			AND
			u.EzPassword = @OldPassword

		-------------------------------------------------------------------------

		UPDATE Security_User SET
			EzPassword = @NewPassword
		WHERE
			UserId = @UserID

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
