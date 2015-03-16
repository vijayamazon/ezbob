IF OBJECT_ID('BrokerLoadOwnProperties2') IS NOT NULL
	DROP PROCEDURE BrokerLoadOwnProperties2
GO

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

	------------------------------------------------------------------------------

	DECLARE @BrokerTermsID INT = NULL

	------------------------------------------------------------------------------

	SET @ContactEmail = ISNULL(LTRIM(RTRIM(ISNULL(@ContactEmail, ''))), '')

	------------------------------------------------------------------------------

	SET @ContactMobile = ISNULL(LTRIM(RTRIM(ISNULL(@ContactMobile, ''))), '')

	------------------------------------------------------------------------------

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

	------------------------------------------------------------------------------

	SELECT TOP 1
		@BrokerTermsID = BrokerTermsID
	FROM
		BrokerTerms
	ORDER BY
		DateAdded DESC

	------------------------------------------------------------------------------
	DECLARE @LinkedBank BIT = 0
	DECLARE @ApprovedAmount DECIMAL(18,2) = 0	
	DECLARE @CommissionAmount DECIMAL(18,2) = 0
	
	IF(@BrokerID > 0)
	BEGIN
		SELECT @LinkedBank = CAST (
			CASE 
				WHEN EXISTS (SELECT * FROM Broker b INNER JOIN CardInfo ci ON b.BrokerID = ci.BrokerID) THEN 1
				ELSE 0
			END AS BIT)
			
		--TODO @ApprovedAmount
		--TODO @CommissionAmount
		
	END
	------------------------------------------------------------------------------

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
		'' AS ErrorMsg,
		ts.BrokerTermsID AS SignedTermsID,
		ts.TermsTextID AS SignedTextID,
		tc.BrokerTermsID AS CurrentTermsID,
		tc.TermsTextID AS CurrentTextID,
		(CASE
			WHEN ts.BrokerTermsID IS NULL OR ts.TermsTextID != tc.TermsTextID THEN tc.BrokerTerms
			ELSE ''
		END) AS CurrentTerms,
		b.LicenseNumber,
		@LinkedBank AS LinkedBank,
		@ApprovedAmount AS ApprovedAmount,
		@CommissionAmount AS CommissionAmount
	FROM
		Broker b
		INNER JOIN BrokerTerms tc ON tc.BrokerTermsID = @BrokerTermsID -- current terms
		LEFT JOIN BrokerTerms ts ON b.BrokerTermsID = ts.BrokerTermsID -- signed terms
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
