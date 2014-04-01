IF OBJECT_ID('BrokerLogin') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLogin AS SELECT 1')
GO

ALTER PROCEDURE BrokerLogin
@Email NVARCHAR(255),
@Password NVARCHAR(255)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ErrMsg NVARCHAR(255),
		@BrokerId INT

	IF NOT EXISTS (SELECT BrokerID FROM Broker WHERE ContactEmail = @Email AND Password = @Password)
	BEGIN
		SET @ErrMsg = 'Invalid contact person email or password.'
		SELECT @ErrMsg AS ErrorMsg
	END
	ELSE
	BEGIN
		SELECT @BrokerId = BrokerID FROM Broker WHERE ContactEmail = @Email AND Password = @Password
		EXECUTE BrokerLoadOwnProperties @Email, @BrokerId
	END		
END
GO
