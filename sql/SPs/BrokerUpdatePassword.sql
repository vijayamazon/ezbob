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

	IF @OldPassword != @NewPassword
	BEGIN
		SELECT
			@BrokerID = BrokerID
		FROM
			Broker
		WHERE
			ContactEmail = @ContactEmail
			AND
			Password = @OldPassword

		UPDATE Broker SET
			Password = @NewPassword
		WHERE
			BrokerID = @BrokerID
	END

	SELECT @BrokerID AS BrokerID
END
GO
