IF OBJECT_ID('BrokerLoadOwnProperties2') IS NOT NULL
	DROP PROCEDURE BrokerLoadOwnProperties2
GO

IF OBJECT_ID('BrokerLoadOwnProperties') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLoadOwnProperties AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE BrokerLoadOwnProperties
@ContactEmail NVARCHAR(255) = '',
@Origin INT = 0,
@BrokerID INT = 0,
@ContactMobile NVARCHAR(255) = ''
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @BrokerTermsID INT = NULL
	DECLARE @LinkedBank BIT = 0
	DECLARE @ApprovedAmount DECIMAL(18, 2) = 0	
	DECLARE @CommissionAmount DECIMAL(18, 2) = 0
	DECLARE @BrokerOriginID INT = NULL

	------------------------------------------------------------------------------

	SET @ContactEmail = ISNULL(LTRIM(RTRIM(ISNULL(@ContactEmail, ''))), '')

	------------------------------------------------------------------------------

	SET @ContactMobile = ISNULL(LTRIM(RTRIM(ISNULL(@ContactMobile, ''))), '')

	------------------------------------------------------------------------------

	IF @ContactEmail != ''
	BEGIN
		IF @ContactMobile != '' OR @BrokerID > 0 OR ISNULL(@Origin, 0) <= 0
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

	IF @BrokerID = 0 OR @BrokerID IS NULL
	BEGIN 
		SET @BrokerID = (
			SELECT TOP 1 b.BrokerID
			FROM Broker b
			WHERE (b.ContactEmail = @ContactEmail AND b.OriginID = @Origin)
			OR b.ContactMobile = @ContactMobile
		)

		SET @BrokerID = ISNULL(@BrokerID, 0)
	END

	------------------------------------------------------------------------------

	SELECT
		@BrokerOriginID = OriginID
	FROM
		Broker
	WHERE
		BrokerID = @BrokerID

	------------------------------------------------------------------------------

	SELECT TOP 1
		@BrokerTermsID = bt.BrokerTermsID
	FROM
		BrokerTerms bt
	WHERE
		bt.OriginID = @BrokerOriginID
	ORDER BY
		bt.DateAdded DESC

	------------------------------------------------------------------------------

	IF @BrokerID > 0
	BEGIN
		SELECT @LinkedBank = CAST(CASE
			WHEN EXISTS (SELECT 1 FROM CardInfo ci WHERE ci.BrokerID = @BrokerID AND ci.IsDefault = 1) THEN 1
			ELSE 0
		END AS BIT)
		
		SELECT @CommissionAmount = ISNULL(SUM(lb.CommissionAmount), 0)
		FROM LoanBrokerCommission lb
		WHERE lb.BrokerID = @BrokerID
		
		SELECT @ApprovedAmount = ISNULL(SUM(cr.ManagerApprovedSum), 0)
		FROM Customer c
		LEFT JOIN Loan l ON l.CustomerId = c.Id AND l.Position = 0
		LEFT JOIN CashRequests cr ON cr.Id = l.RequestCashId
		WHERE c.BrokerID = @BrokerID
		AND c.OriginID = @BrokerOriginID
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
		@CommissionAmount AS CommissionAmount,
		ci.BankAccount AS BankAccount,
		ci.SortCode AS BankSortCode,
		ci.Bank AS BankName,
		b.OriginID,
		bo.Name AS Origin,
		bo.FrontendSite
	FROM
		Broker b
		INNER JOIN CustomerOrigin bo ON b.OriginID = bo.CustomerOriginID
		INNER JOIN BrokerTerms tc ON tc.BrokerTermsID = @BrokerTermsID -- current terms
		LEFT JOIN BrokerTerms ts ON b.BrokerTermsID = ts.BrokerTermsID -- signed terms
		LEFT JOIN CardInfo ci ON b.BrokerID=ci.BrokerID AND ci.IsDefault = 1
	WHERE
		b.BrokerID = @BrokerID
	ORDER BY
		b.BrokerID DESC
END
GO
