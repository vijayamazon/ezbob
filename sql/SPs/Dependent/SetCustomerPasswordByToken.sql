IF OBJECT_ID('SetCustomerPasswordByToken') IS NULL
	EXECUTE('CREATE PROCEDURE SetCustomerPasswordByToken AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE SetCustomerPasswordByToken
@UserID INT,
@Token UNIQUEIDENTIFIER,
@EzPassword VARCHAR(255),
@Salt VARCHAR(255),
@CycleCount VARCHAR(255),
@IsBrokerLead BIT,
@Ip NVARCHAR(50),
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @SessionID INT = 0
	DECLARE @IsDisabled BIT = 0
	DECLARE @IsBroker BIT = 0

	------------------------------------------------------------------------------

	BEGIN TRANSACTION

	------------------------------------------------------------------------------

	IF @IsBrokerLead = 0
	BEGIN
		UPDATE CreatePasswordTokens SET
			DateDeleted = @Now
		WHERE
			TokenID = @Token
	END
	ELSE BEGIN
		UPDATE BrokerLeadTokens SET
			DateDeleted = @Now
		WHERE
			BrokerLeadToken = @Token
	END

	------------------------------------------------------------------------------

	UPDATE Security_User SET
		EzPassword = @EzPassword,
		Salt = @Salt,
		CycleCount = @CycleCount
	WHERE
		UserId = @UserID

	------------------------------------------------------------------------------

	SET @IsBroker = CASE WHEN EXISTS (SELECT BrokerID FROM Broker WHERE BrokerID = @UserID) THEN 1 ELSE 0 END

	------------------------------------------------------------------------------

	IF @IsBroker = 0
	BEGIN
		SET @IsDisabled = CASE
			WHEN EXISTS (SELECT c.Id FROM Customer c INNER JOIN CustomerStatuses s ON c.CollectionStatus = s.Id AND s.Name = 'Disabled' WHERE c.Id = @UserID)
				THEN 1
			ELSE 0
		END
	END

	------------------------------------------------------------------------------

	DECLARE @SessionMessage NVARCHAR(50) = (CASE @IsDisabled WHEN  1 THEN 'User account is disabled' ELSE 'Password restored' END)

	EXECUTE CreateCustomerSession @UserID, @Now, @Ip, 1, @SessionMessage, NULL, @SessionID OUTPUT

	------------------------------------------------------------------------------

	COMMIT TRANSACTION

	------------------------------------------------------------------------------

	SELECT
		SessionID = @SessionID,
		IsBroker = @IsBroker,
		IsDisabled = @IsDisabled,
		OriginName = o.Name,
		o.CustomerCareEmail
	FROM
		Security_User u
		INNER JOIN CustomerOrigin o ON u.OriginID = o.CustomerOriginID
END
GO
