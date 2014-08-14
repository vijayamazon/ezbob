IF OBJECT_ID('BrokerLoadSignedTerms') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLoadSignedTerms AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE BrokerLoadSignedTerms
@ContactEmail NVARCHAR(255)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1
		b.ContactEmail,
		b.AgreedToTermsDate AS SignedTime,
		t.BrokerTerms AS Terms
	FROM
		Broker b
		INNER JOIN BrokerTerms t ON b.BrokerTermsID = t.BrokerTermsID
	WHERE
		b.ContactEmail = @ContactEmail
END
GO
