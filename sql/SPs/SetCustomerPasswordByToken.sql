IF OBJECT_ID('SetCustomerPasswordByToken') IS NULL
	EXECUTE('CREATE PROCEDURE SetCustomerPasswordByToken AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE SetCustomerPasswordByToken
@Email NVARCHAR(128),
@EzPassword VARCHAR(255),
@TokenID UNIQUEIDENTIFIER,
@IsBrokerLead BIT,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @CustomerID INT = 0
	DECLARE @BrokerLeadTokenID INT = 0

	------------------------------------------------------------------------------

	IF @IsBrokerLead = 0
	BEGIN
		SELECT
			@CustomerID = c.UserId
		FROM
			Security_User c
			INNER JOIN CreatePasswordTokens t
				ON c.UserId = t.CustomerID
				AND t.TokenID = @TokenID
				AND t.DateAccessed IS NOT NULL
				AND t.DateDeleted IS NULL
		WHERE
			c.Email = @Email
	END
	ELSE BEGIN
		SELECT
			@CustomerID = l.CustomerID,
			@BrokerLeadTokenID = t.BrokerLeadTokenID
		FROM
			BrokerLeads l
			INNER JOIN BrokerLeadTokens t
				ON l.BrokerLeadID = t.BrokerLeadID
				AND t.BrokerLeadToken = @TokenID
				AND l.Email = @Email
				AND t.DateAccessed IS NOT NULL
				AND t.DateDeleted IS NULL
	END

	------------------------------------------------------------------------------

	IF @CustomerID > 0
	BEGIN
		BEGIN TRANSACTION

		-------------------------------------------------------------------------

		IF @IsBrokerLead = 0
		BEGIN
			UPDATE CreatePasswordTokens SET
				DateDeleted = @Now
			WHERE
				TokenID = @TokenID
		END
		ELSE BEGIN
			UPDATE BrokerLeadTokens SET
				DateDeleted = @Now
			WHERE
				BrokerLeadTokenID = @BrokerLeadTokenID
		END

		-------------------------------------------------------------------------

		UPDATE Security_User SET
			EzPassword = @EzPassword
		WHERE
			UserId = @CustomerID

		-------------------------------------------------------------------------

		SELECT @CustomerID AS CustomerID

		-------------------------------------------------------------------------

		COMMIT TRANSACTION

		-------------------------------------------------------------------------

		RETURN
	END

	------------------------------------------------------------------------------

	SELECT CONVERT(INT, 0) AS CustomerID
END
GO
