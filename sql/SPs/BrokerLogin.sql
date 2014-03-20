IF OBJECT_ID('BrokerLogin') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLogin AS SELECT 1')
GO

ALTER PROCEDURE BrokerLogin
@Email NVARCHAR(255),
@Password NVARCHAR(255)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ErrMsg NVARCHAR(255) = ''

	IF @ErrMsg = ''
	BEGIN
		IF NOT EXISTS (SELECT * FROM Broker WHERE ContactEmail = @Email AND Password = @Password)
			SET @ErrMsg = 'Invalid contact person email or password.'
	END

	SELECT @ErrMsg AS ErrorMsg
END
GO
