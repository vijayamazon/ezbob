IF OBJECT_ID('BrokerLoadLeadList') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLoadLeadList AS SELECT 1')
GO

ALTER PROCEDURE BrokerLoadLeadList
@ContactEmail NVARCHAR(255)
AS
BEGIN
	DECLARE @BrokerID INT

	SELECT @BrokerID = BrokerID FROM Broker WHERE ContactEmail = @ContactEmail

	SELECT
		bl.BrokerLeadID AS LeadID,
		bl.FirstName,
		bl.LastName,
		bl.Email,
		m.BrokerLeadAddMode AS AddMode,
		bl.DateCreated,
		bl.DateLastInvitationSent
	FROM
		BrokerLeads bl
		INNER JOIN BrokerLeadAddModes m ON bl.BrokerLeadAddModeID = m.BrokerLeadAddModeID
	ORDER BY
		bl.BrokerLeadID
END
GO
