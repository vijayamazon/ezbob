IF OBJECT_ID('BrokerUpdatePassword') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerUpdatePassword AS SELECT 1')
GO

ALTER PROCEDURE BrokerUpdatePassword
@BrokerID INT,
@NewPassword NVARCHAR(255),
@Salt NVARCHAR(255),
@CycleCount NVARCHAR(255)
AS
BEGIN
	UPDATE Security_User SET
		EzPassword = @NewPassword,
		Salt = @Salt,
		CycleCount = @CycleCount
	WHERE
		UserId = @BrokerID

	-------------------------------------------------------------------------

	UPDATE Broker SET
		Password = 'not used'
	WHERE
		BrokerID = @BrokerID
END
GO
