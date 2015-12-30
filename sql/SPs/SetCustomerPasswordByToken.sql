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
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

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

	COMMIT TRANSACTION
END
GO
