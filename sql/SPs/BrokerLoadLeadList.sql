IF OBJECT_ID('BrokerLoadLeadList') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLoadLeadList AS SELECT 1')
GO

ALTER PROCEDURE BrokerLoadLeadList
@ContactEmail NVARCHAR(255),
@Origin INT,
@BrokerID INT
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	IF @BrokerID IS NULL OR @BrokerID <= 0
		SELECT @BrokerID = BrokerID FROM Broker WHERE ContactEmail = @ContactEmail AND OriginID = @Origin

	------------------------------------------------------------------------------

	SELECT
		bl.BrokerLeadID AS LeadID,
		bl.CustomerID,
		bl.FirstName,
		bl.LastName,
		bl.Email,
		m.BrokerLeadAddMode AS AddMode,
		bl.DateCreated,
		bl.DateLastInvitationSent,
		CONVERT(BIT, (CASE
			WHEN bl.DateDeleted IS NULL THEN 0
			ELSE 1
		END)) AS IsDeleted
	FROM
		BrokerLeads bl
		INNER JOIN BrokerLeadAddModes m ON bl.BrokerLeadAddModeID = m.BrokerLeadAddModeID
	WHERE
		bl.BrokerID = @BrokerID
	ORDER BY
		bl.BrokerLeadID
END
GO
