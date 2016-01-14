SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('BrokerUpdateEmail') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerUpdateEmail AS SELECT 1')
GO

ALTER PROCEDURE BrokerUpdateEmail
@ChangedByUserID INT,
@BrokerID INT,
@NewEmail NVARCHAR(255),
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	SET @NewEmail = LOWER(ISNULL(@NewEmail, ''))

	DECLARE @ErrMsg NVARCHAR(1024) = ''

	DECLARE @OriginID INT
	DECLARE @OldEmail NVARCHAR(250)
	
	SELECT
		@OriginID = OriginID,
		@OldEmail = ContactEmail
	FROM
		Broker
	WHERE
		BrokerID = @BrokerID

	IF @OriginID IS NULL OR @OriginID < 0
		SET @ErrMsg = 'Failed to find broker by id ' + @BrokerID

	IF @ErrMsg = ''
		SET @ErrMsg = dbo.udfCheckEmailUniqueness(@NewEmail, @OriginID, DEFAULT, DEFAULT, DEFAULT)

	IF @ErrMsg = ''
		SET @ErrMsg = dbo.udfCheckExternalBrokerEmailCollissions(@NewEmail)

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

			INSERT INTO UserEmailHistory (EventTime, ChangedByUserID, UserID, OldEmail, NewEmail)
				VALUES (@Now, @ChangedByUserID, @BrokerID, @OldEmail, @NewEmail)

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
