IF OBJECT_ID('RptBroker') IS NULL
	EXECUTE('CREATE PROCEDURE RptBroker AS SELECT 1')
GO

ALTER PROCEDURE RptBroker
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	SELECT 
		B.ContactName Name,
		B.FirmName Company,
		B.ContactMobile Mobile,
		B.ContactOtherPhone Phone,
		B.ContactEmail Email,
		B.AgreedToTermsDate SignUpDate
	FROM 
		Broker B
	WHERE
		@DateStart <= B.AgreedToTermsDate AND B.AgreedToTermsDate < @DateEnd
		AND
		B.IsTest = 0
	ORDER BY
		B.AgreedToTermsDate DESC
END
GO
