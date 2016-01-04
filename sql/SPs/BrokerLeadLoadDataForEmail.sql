SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('BrokerLeadLoadDataForEmail') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLeadLoadDataForEmail AS SELECT 1')
GO

ALTER PROCEDURE BrokerLeadLoadDataForEmail
@LeadID INT,
@ContactEmail NVARCHAR(255),
@OriginID INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @BrokerID INT
	DECLARE @FirmName NVARCHAR(255)
	DECLARE @Origin NVARCHAR(20)
	DECLARE @OriginSite NVARCHAR(255)

	SELECT
		@BrokerID   = b.BrokerID,
		@FirmName   = b.FirmName,
		@Origin     = o.Name,
		@OriginSite = o.CustomerSite
	FROM
		Broker b
		INNER JOIN CustomerOrigin o ON b.OriginID = o.CustomerOriginID
	WHERE
		b.ContactEmail = @ContactEmail
		AND
		b.OriginID = @OriginID

	SELECT
		bl.BrokerLeadID AS LeadID,
		bl.FirstName,
		bl.LastName,
		bl.Email,
		@FirmName AS FirmName,
		@Origin AS Origin,
		@OriginSite AS OriginSite
	FROM
		BrokerLeads bl
	WHERE
		bl.BrokerLeadID = @LeadID
		AND
		bl.BrokerID = @BrokerID
		AND
		bl.DateDeleted IS NULL
		AND
		bl.BrokerLeadDeletedReasonID IS NULL
END
GO
