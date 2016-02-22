IF OBJECT_ID('BrokerAcceptTerms') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerAcceptTerms AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE BrokerAcceptTerms
@TermsID INT,
@ContactEmail NVARCHAR(255),
@OriginID INT,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE Broker SET
		BrokerTermsID = @TermsID,
		AgreedToTermsDate = @Now
	WHERE
		ContactEmail = @ContactEmail
		AND
		OriginID = @OriginID
END
GO
