SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('BrokerLoadContactData') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLoadContactData AS SELECT 1')
GO

ALTER PROCEDURE BrokerLoadContactData
@BrokerID INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		b.BrokerID,
		b.FirmName,
		b.ContactName,
		b.ContactEmail,
		b.BrokerID AS UserID,
		o.Name AS Origin,
		o.CustomerSite
	FROM
		Broker b
		INNER JOIN CustomerOrigin o ON b.OriginID = o.CustomerOriginID
	WHERE
		b.BrokerID = @BrokerID
END
GO
