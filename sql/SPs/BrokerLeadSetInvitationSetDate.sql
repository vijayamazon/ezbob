IF OBJECT_ID('BrokerLeadSetInvitationSetDate') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLeadSetInvitationSetDate AS SELECT 1')
GO

ALTER PROCEDURE BrokerLeadSetInvitationSetDate
@LeadID INT,
@DateInvitationSent DATETIME
AS
	SET NOCOUNT ON;

	UPDATE BrokerLeads SET
		DateLastInvitationSent = @DateInvitationSent
	WHERE
		BrokerLeadID = @LeadID
		AND (
			DateLastInvitationSent IS NULL
			OR
			DateLastInvitationSent < @DateInvitationSent
		)
		AND
		DateDeleted IS NULL
		AND
		BrokerLeadDeletedReasonID IS NULL
GO
