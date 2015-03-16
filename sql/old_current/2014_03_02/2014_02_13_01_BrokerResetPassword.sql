IF OBJECT_ID('BrokerResetPassword') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerResetPassword AS SELECT 1')
GO

ALTER PROCEDURE BrokerResetPassword
@ContactMobile NVARCHAR(255),
@Password NVARCHAR(255)
AS
BEGIN
	DECLARE @ErrMsg NVARCHAR(255) = ''
	DECLARE @BrokerID INT = 0

	IF @ErrMsg = ''
	BEGIN
		SELECT @BrokerID = BrokerID FROM Broker WHERE ContactMobile = @ContactMobile

		IF @BrokerID IS NULL
		BEGIN
			SET @BrokerID = 0
			SET @ErrMsg = 'Could not find broker by mobile ' + @ContactMobile
		END
	END

	IF @ErrMsg = ''
	BEGIN
		UPDATE Broker SET
			Password = @Password
		WHERE
			BrokerID = @BrokerID
	END

	SELECT @ErrMsg AS ErrorMsg, @BrokerID AS BrokerID
END
GO
