IF OBJECT_ID('BrokerLoadOwnProperties') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLoadOwnProperties AS SELECT 1')
GO

ALTER PROCEDURE BrokerLoadOwnProperties
@ContactEmail NVARCHAR(255),
@BrokerID INT
AS
BEGIN
	SET NOCOUNT ON;

	SET @ContactEmail = ISNULL(LTRIM(RTRIM(ISNULL(@ContactEmail, ''))), '')

	SELECT
		b.BrokerID,
		b.FirmName AS BrokerName,
		b.FirmRegNum AS BrokerRegNum,
		b.ContactName,
		b.ContactEmail,
		b.ContactMobile,
		b.ContactOtherPhone,
		b.SourceRef,
		b.FirmWebSiteUrl AS BrokerWebSiteUrl,
		'' AS ErrorMsg
	FROM
		Broker b
	WHERE
		(@ContactEmail != '' AND b.ContactEmail = @ContactEmail)
		OR
		(@BrokerID > 0 AND b.BrokerID = @BrokerID)
END
GO


IF OBJECT_ID('BrokerLoadOwnProperties2') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLoadOwnProperties2 AS SELECT 1')
GO

ALTER PROCEDURE BrokerLoadOwnProperties2
@Mobile NVARCHAR(255)
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @BrokerId INT, @ContactEmail NVARCHAR(255)
	SELECT TOP 1 @BrokerId = BrokerID, @ContactEmail = ContactEmail FROM Broker WHERE ContactMobile = @Mobile ORDER BY BrokerID DESC
	
	EXECUTE BrokerLoadOwnProperties @ContactEmail, @BrokerId
END
GO