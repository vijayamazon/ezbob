IF OBJECT_ID('BrokerLeadCheckToken') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLeadCheckToken AS SELECT 1')
GO

ALTER PROCEDURE BrokerLeadCheckToken
@Token UNIQUEIDENTIFIER,
@DateDeleted DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @LeadID INT

	------------------------------------------------------------------------------

	SELECT
		@LeadID = BrokerLeadID
	FROM
		BrokerLeadTokens
	WHERE
		BrokerLeadToken = @Token
		AND
		DateAccessed IS NULL
		AND
		DateDeleted IS NULL

	------------------------------------------------------------------------------

	IF @LeadID IS NOT NULL
	BEGIN
		UPDATE BrokerLeadTokens SET
			DateAccessed = @DateDeleted
		WHERE
			BrokerLeadID = @LeadID
			AND
			DateAccessed IS NULL
			AND
			DateDeleted IS NULL

		-------------------------------------------------------------------------

		SELECT
			BrokerLeadID AS LeadID,
			Email AS LeadEmail,
			CustomerID,
			FirstName,
			LastName
		FROM
			BrokerLeads
		WHERE
			BrokerLeadID = @LeadID
			AND
			DateDeleted IS NULL
			AND
			BrokerLeadDeletedReasonID IS NULL
	END

	------------------------------------------------------------------------------
END
GO
