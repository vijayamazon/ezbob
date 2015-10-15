SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('BrokerUpdateEmail') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerUpdateEmail AS SELECT 1')
GO

ALTER PROCEDURE BrokerUpdateEmail
@BrokerID INT,
@NewEmail NVARCHAR(255)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ErrMsg NVARCHAR(1024) = ''

	IF @ErrMsg = ''
		SET @ErrMsg = dbo.udfCheckEmailUniqueness(@NewEmail, DEFAULT, DEFAULT, DEFAULT)

	IF @ErrMsg = ''
	BEGIN
		BEGIN TRY
			BEGIN TRANSACTION

			UPDATE Security_User SET
				UserName = @NewEmail,
				Email = @NewEmail
			WHERE
				UserID = @BrokerID

			UPDATE Broker SET
				ContactEmail = @NewEmail
			WHERE
				BrokerID = @BrokerID

			SET @ErrMsg = ''

			COMMIT TRANSACTION
		END TRY
		BEGIN CATCH
			SET @ErrMsg = dbo.udfGetErrorMsg()

			ROLLBACK TRANSACTION
		END CATCH
	END

	SELECT 'Result' = @ErrMsg
END
GO
