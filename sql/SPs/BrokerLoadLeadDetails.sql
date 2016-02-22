IF OBJECT_ID('BrokerLoadLeadDetails') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLoadLeadDetails AS SELECT 1')
GO

ALTER PROCEDURE BrokerLoadLeadDetails
@LeadID INT,
@ContactEmail NVARCHAR(255),
@Origin INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @BrokerID INT

	SELECT
		@BrokerID = BrokerID
	FROM
		Broker
	WHERE
		ContactEmail = @ContactEmail
		AND
		OriginID = @Origin

	SELECT
		l.BrokerLeadID AS Id,
		l.FirstName,
		l.LastName,
		l.Email,
		l.DateCreated,
		l.DateLastInvitationSent
	FROM
		BrokerLeads l
	WHERE
		l.BrokerID = @BrokerID
		AND
		l.BrokerLeadID = @LeadID
END
GO