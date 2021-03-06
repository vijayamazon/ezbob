IF OBJECT_ID('UwGridBrokers') IS NULL
	EXECUTE('CREATE PROCEDURE UwGridBrokers AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UwGridBrokers
@WithTest BIT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		b.BrokerID,
		b.FirmName,
		b.ContactName,
		b.ContactEmail,
		b.ContactMobile,
		b.ContactOtherPhone,
		b.FirmWebSiteUrl,
		b.IsTest,
		b.OriginID,
		o.Name AS Origin
	FROM
		Broker b
		INNER JOIN CustomerOrigin o ON b.OriginID = o.CustomerOriginID
	WHERE
		@WithTest = 1
		OR
		b.IsTest = 0
	ORDER BY
		b.FirmName
END
GO
