IF OBJECT_ID('BrokerResetPassword') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerResetPassword AS SELECT 1')
GO

ALTER PROCEDURE BrokerResetPassword
@BrokerID INT,
@Password NVARCHAR(255)
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE Broker SET
		Password = @Password
	WHERE
		BrokerID = @BrokerID
END
GO
