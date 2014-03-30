IF OBJECT_ID('BrokerLoadOwnProperties') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLoadOwnProperties AS SELECT 1')
GO

ALTER PROCEDURE BrokerLoadOwnProperties
@ContactEmail NVARCHAR(255)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		b.BrokerID,
		b.FirmName AS BrokerName,
		b.FirmRegNum AS BrokerRegNum,
		b.ContactName,
		b.ContactEmail,
		b.ContactMobile,
		b.ContactOtherPhone,
		b.SourceRef,
		b.FirmWebSiteUrl AS BrokerWebSiteUrl
	FROM
		Broker b
	WHERE
		ContactEmail = @ContactEmail
END
GO
