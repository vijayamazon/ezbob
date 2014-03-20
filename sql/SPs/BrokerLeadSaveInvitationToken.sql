IF OBJECT_ID('BrokerLeadSaveInvitationToken') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLeadSaveInvitationToken AS SELECT 1')
GO

ALTER PROCEDURE BrokerLeadSaveInvitationToken
@LeadID INT,
@Token UNIQUEIDENTIFIER,
@DateCreated DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	IF EXISTS (
		SELECT
			BrokerLeadID
		FROM
			BrokerLeads
		WHERE
			DateDeleted IS NULL
			AND
			BrokerLeadDeletedReasonID IS NULL
	)
	BEGIN
		INSERT INTO BrokerLeadTokens (BrokerLeadID, BrokerLeadToken, DateCreated)
			VALUES (@LeadID, @Token, @DateCreated)
	END
END
GO
