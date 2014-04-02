IF OBJECT_ID('BrokerLoadOwnProperties') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLoadOwnProperties AS SELECT 1')
GO

ALTER PROCEDURE BrokerLoadOwnProperties
@ContactEmail NVARCHAR(255) = '',
@BrokerID INT = 0,
@ContactMobile NVARCHAR(255) = ''
AS
BEGIN
	SET NOCOUNT ON;

	SET @ContactEmail = ISNULL(LTRIM(RTRIM(ISNULL(@ContactEmail, ''))), '')

	SET @ContactMobile = ISNULL(LTRIM(RTRIM(ISNULL(@ContactMobile, ''))), '')

	IF @ContactEmail != ''
	BEGIN
		IF @ContactMobile != '' OR @BrokerID > 0
			RAISERROR('Invalid arguments: broker id or contact mobile is set when contact email is set.', 11, 1)
	END
	ELSE BEGIN
		IF @BrokerID > 0
		BEGIN
			IF @ContactMobile != ''
				RAISERROR('Invalid arguments: contact mobile is set when broker id is set.', 11, 2)
		END
		ELSE BEGIN
			IF @ContactMobile = ''
				RAISERROR('Invalid arguments: no broker identifier set.', 11, 3)
		END
	END

	SELECT TOP 1
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
		b.ContactEmail = @ContactEmail
		OR
		b.BrokerID = @BrokerID
		OR
		b.ContactMobile = @ContactMobile
	ORDER BY
		b.BrokerID DESC
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
